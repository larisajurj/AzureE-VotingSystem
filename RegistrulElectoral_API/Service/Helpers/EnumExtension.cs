﻿using System.ComponentModel;
using System.Reflection;

namespace Service.Helpers;

public static class EnumExtension
{
	public static string GetDescription(this Enum value)
	{
		var field = value.GetType().GetField(value.ToString());
		var attr = field?.GetCustomAttribute<DescriptionAttribute>();
		return attr?.Description ?? value.ToString();
	}
}