using System;
using System.Collections.Generic;
using MBC.Core.Entities;

namespace MBC.Services.Seeds.Data;

public static class Sites
{
    public static IEnumerable<Site> Seeds =>
    [
        new Site
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Name = "Mango Bay Freight Terminal",
            Notes = "Main cargo hub with excellent harbor facilities and mountain views",
            Island = "Mango Bay",
            Address = "1 Harbor Drive, Mango Bay",
            Location = new Location { X = 120, Y = 200 },
            Status = SiteStatus.Current,
            ImageUrl = "sites/mango-bay.webp"
        },
        new Site
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Name = "Coconut Cove Trading Post",
            Notes = "Remote island outpost perfect for exotic cargo shipments",
            Island = "Coconut Island",
            Address = "Coconut Beach, Coconut Island",
            Location = new Location { X = 45, Y = 67 },
            Status = SiteStatus.Current,
            ImageUrl = "sites/coconut-cove.webp"
        },
        new Site
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Frost Peak Command Depot",
            Notes = "Frozen tundra location with limited seasonal access",
            Island = "Frost Peak Island",
            Address = "Central Square, Frost Peak Island",
            Location = new Location { X = 210, Y = 15 },
            Status = SiteStatus.Inactive,
            ImageUrl = "sites/frost-peak.webp"
        },
        new Site
        {
            Id = new Guid("44444444-4444-4444-4444-444444444444"),
            Name = "Jade Summit Airstrip",
            Notes = "Hidden mountain kingdom with challenging approach",
            Island = "Jade Mountain",
            Address = "Royal Plaza, Jade Mountain",
            Location = new Location { X = 88, Y = 180 },
            Status = SiteStatus.Current,
            ImageUrl = "sites/jade-summit.webp"
        },
        new Site
        {
            Id = new Guid("55555555-5555-5555-5555-555555555555"),
            Name = "Coral Harbor Express",
            Notes = "Bustling marketplace specializing in rare goods",
            Island = "Coral Bay",
            Address = "Market Street, Coral Bay",
            Location = new Location { X = 155, Y = 92 },
            Status = SiteStatus.Current,
            ImageUrl = "sites/coral-harbor.webp"
        },
        new Site
        {
            Id = new Guid("66666666-6666-6666-6666-666666666666"),
            Name = "Paradise Lagoon Cargo Facility",
            Notes = "Tropical paradise with limited infrastructure",
            Island = "Paradise Island",
            Address = "Lagoon Road, Paradise Island",
            Location = new Location { X = 33, Y = 220 },
            Status = SiteStatus.Upcoming,
            ImageUrl = "sites/paradise-lagoon.webp"
        },
        new Site
        {
            Id = new Guid("88888888-8888-8888-8888-888888888888"),
            Name = "Dawn Harbor Seaplane Base",
            Notes = "Under construction, scheduled to open next quarter",
            Island = "Dawn Island",
            Address = "Waterfront Promenade, Dawn Island",
            Location = new Location { X = 78, Y = 140 },
            Status = SiteStatus.Upcoming,
            ImageUrl = "sites/dawn-harbor.webp"
        }
    ];
}
