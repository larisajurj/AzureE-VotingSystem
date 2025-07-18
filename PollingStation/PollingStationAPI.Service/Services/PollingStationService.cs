﻿using Microsoft.Azure.Cosmos.Linq;
using PollingStationAPI.Data.Models;
using PollingStationAPI.Data.Repository.Abstractions;
using PollingStationAPI.Service.Exceptions;
using PollingStationAPI.Service.Services.Abstractions;

namespace PollingStationAPI.Service.Services;

public class PollingStationService : IPollingStationService
{
    private readonly IRepository<PollingStation,string> _repository;

    public PollingStationService(IRepository<PollingStation, string> repository)
    {
        _repository = repository;
    }

    public async Task<Booth> AddBooth(string pollingStationId)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null) {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }
        if (pollingStation.MaxBooths <= pollingStation.Booths.Count)
        {
            throw new Exception("Max booths already registered");
        }
        int latestBoothNumber = pollingStation.Booths.Any()
                                    ? pollingStation.Booths.Max(b => b.Id)
                                    : 0;
        var newBooth = new Booth()
        {
            Id = latestBoothNumber + 1,
            Status = "locked"
        };

        pollingStation.Booths.Add(newBooth);
        await _repository.Update(pollingStation);
        return newBooth;

    }

    public async void AddTestData()
    {
        var pollingStation = new PollingStation
        {
            Id = "2",
            Name = "Strada Steagului",
            MaxBooths = 2,
            Siruta = 1,
        };

        await _repository.Add(pollingStation);
    }

    public async Task DeleteSession(int boothId, string pollingStationId)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null)
        {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }
        var booth = pollingStation.Booths.Where(b => b.Id == boothId).FirstOrDefault();

        if (booth == null)
        {
            throw new NotFoundException($"Booth was not registered");
        }
        booth.SessionId = null;
        booth.Status = "locked";
        await _repository.Update(pollingStation);
    }

    public async Task<Booth> GetBooth(string pollingStationId, int boothId)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null)
        {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }
        var booth = pollingStation.Booths.Where(b => b.Id.Equals(boothId)).FirstOrDefault();

        if (booth == null)
        {
            throw new NotFoundException($"Booth '{boothId}' not found.");
        }
        return booth;

    }

    public async Task<Booth> UpdateStatusBooth(string pollingStationId, int boothId, string state)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null)
        {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }
        var booth = pollingStation.Booths.Where(b => b.Id.Equals(boothId)).FirstOrDefault();

        if (booth == null)
        {
            throw new NotFoundException($"Booth '{boothId}' not found.");
        }
        booth.Status = state;
        await _repository.Update(pollingStation);

        return booth;

    }

    public async Task<PollingStation> GetPollingStation(string pollingStationId)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null)
        {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }
        return pollingStation;
    }

    //Register a booth session
    public async Task<Booth> RegisterSession(string sessionId, string pollingStationId)
    {
        PollingStation? pollingStation = await _repository.GetById(pollingStationId);
        if (pollingStation == null)
        {
            throw new NotFoundException($"Polling station '{pollingStationId}' not found.");
        }

        //Check if there is a booth associated with this session number
        if (pollingStation.Booths.Where(b => b.SessionId == sessionId).Any())
        {
            Console.WriteLine($"This session already has a registered booth");
            return pollingStation.Booths.Where(b => b.SessionId == sessionId).FirstOrDefault() ?? throw new Exception("Could not register");
        }

        //Check if there is a booth without a registered session
        var booth = pollingStation.Booths.Where(b => b.SessionId == null).FirstOrDefault();

        if (booth == null)
        {
            throw new NotFoundException($"All booths have registered sessions");
        }

        booth.SessionId = sessionId;
        booth.Status = "locked";
        await _repository.Update(pollingStation);

        return booth;
    }

    public async Task<PollingStation> GetPollingStationByUserId(string userId)
    {
        var pollingStations = await _repository.Filter(ps => ps.CommitteeMemberIds.Contains(userId));
        if (pollingStations == null || pollingStations.Count() == 0)
        {
            throw new NotFoundException($"Polling station associated with '{userId}' not found.");
        }
        var pollingStation = pollingStations.FirstOrDefault();

        return pollingStation ?? throw new NotFoundException($"Polling station associated with '{userId}' not found.");
    }

    public async Task<PollingStation?> UpdateAsync(PollingStation entity)
    {
        return await _repository.Update(entity);
    }
}
