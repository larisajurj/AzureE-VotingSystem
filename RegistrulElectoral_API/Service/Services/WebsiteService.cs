using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Service.Helpers;
using Service.Models;
using Service.Models.Enums;
using Service.Services.Abstractions;
using Newtonsoft.Json.Linq;


namespace Service.Services;

public class WebsiteService : IWebsiteService
{
	private readonly HttpClient client;
	public WebsiteService(IHttpClientFactory httpClientFactory)
	{
		client = httpClientFactory.CreateClient();
	}
	public async Task<RegistrationStatusDTO> checkRegistration(String cnp, String lastName, string apikey)
	{
		IWebDriver driver = new ChromeDriver();
		// Go to the web page
		driver.Navigate().GoToUrl("https://www.registrulelectoral.ro/");

		var pageUrl = driver.Url;
		var siteKey = driver.FindElement(By.CssSelector("div.g-recaptcha")).GetAttribute("data-sitekey");
		var captchaResponse = await SolveCaptcha(siteKey, pageUrl, apikey);

		IWebElement recaptchaResponseElement = driver.FindElement(By.Id("g-recaptcha-response"));
		((IJavaScriptExecutor)driver).ExecuteScript($"arguments[0].value = '{captchaResponse}';", recaptchaResponseElement);

		// Find the input field and type in it
		IWebElement cnpField = driver.FindElement(By.Name("CNP")); // Use Name, Id, ClassName, etc.
		cnpField.SendKeys(cnp);
		IWebElement lastNameField = driver.FindElement(By.Name("NUME")); // Use Name, Id, ClassName, etc.
		lastNameField.SendKeys(lastName);
		IWebElement submitButton = driver.FindElement(By.XPath("/html/body/div[3]/div[1]/div/div[2]/div[1]/div/div/div[5]/a"));
		submitButton.Click();
		Thread.Sleep(1000);

		var toastElements = driver.FindElements(By.ClassName("toast-message"));
		var nume = toastElements.First().Text;

		if (toastElements.Any())
		{
			driver.Quit();
			return new RegistrationStatusDTO()
			{
				CNP = cnp,
				LastName = lastName,
				Details = toastElements.First().Text,
				Status = RegistrationStatusDetails.SuccessfullValidation
			};
		}
		var youAreRegisteredMessage = driver.FindElements(By.XPath("/html/body/div[3]/section/div/div[2]/div/h2"));
		if (youAreRegisteredMessage.Any() && youAreRegisteredMessage.First().Text.Contains("Sunteți înscris/ă pe listele permanente ale secției de votare nr."))
		{
			driver.Quit();
			return new RegistrationStatusDTO()
			{
				CNP = cnp,
				LastName = lastName,
				Details = EnumExtension.GetDescription(RegistrationStatusDetails.SuccessfullValidation),
				Status = RegistrationStatusDetails.SuccessfullValidation
			};
		}

		Thread.Sleep(10000);

		driver.Quit();

		return new RegistrationStatusDTO() { 
			CNP = cnp, 
			LastName = lastName, 
			Details = EnumExtension.GetDescription(RegistrationStatusDetails.SuccessfullValidation), 
			Status = RegistrationStatusDetails.SuccessfullValidation
		};
	}

	static async Task<string> SolveCaptcha(string siteKey, string pageUrl, string apikey)
	{
		var client = new HttpClient();
		var content = new StringContent($"{{\"clientKey\": \"{apikey}\", \"task\": {{\"type\": \"RecaptchaV2TaskProxyless\", \"websiteURL\": \"{pageUrl}\", \"websiteKey\": \"{siteKey}\"}}}}", System.Text.Encoding.UTF8, "application/json");
		var response = await client.PostAsync("https://api.capsolver.com/createTask", content);
		var jsonResponse = await response.Content.ReadAsStringAsync();
		var taskId = JObject.Parse(jsonResponse)["taskId"].ToString();

		string captchaSolution = "";
		string status = "started";
		while (status != "ready")
		{
			await Task.Delay(1000);
			var content2 = new StringContent($"{{\"clientKey\": \"{apikey}\", \"taskId\": \"{taskId}\"}}", System.Text.Encoding.UTF8, "application/json");
			var response2 = await client.PostAsync("https://api.capsolver.com/getTaskResult", content2);
			var jsonResponse3 = await response2.Content.ReadAsStringAsync();
			status = captchaSolution = JObject.Parse(jsonResponse3)["status"].ToString();
			if(status == "ready")
				captchaSolution = JObject.Parse(jsonResponse3)["solution"]["gRecaptchaResponse"].ToString();
			else if (status == "failed")
			{
				throw new Exception(jsonResponse3);
			}
		}

		return captchaSolution;
	}
}
