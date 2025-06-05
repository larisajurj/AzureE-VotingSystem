namespace PollingStationAPI.Service.DTOs;

public record PollingStationInfo(
    string Id,
    string Name,
    string Representative,
    string ATU,
    string Locality
);