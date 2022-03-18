using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;

namespace TicketBurst.SearchService.Controllers;

[ApiController]
[Route("venue")]
public class VenueController : ControllerBase
{
    public VenueController(ILogger<VenueController> logger)
    {
    }

    [HttpGet]
    public IEnumerable<VenueContract> Get()
    {
        return __mockData.Select(x => x with { Halls = null });
    }

    [HttpGet("{id}/halls")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<IEnumerable<HallContract>> GetHalls(string id)
    {
        var result =  __mockData.FirstOrDefault(v => v.Id == id)?.Halls;
        
        if (result != null)
        {
            return result;
        }

        return NotFound();
    }
    
    private static readonly VenueContract[] __mockData = new[] {
        new VenueContract(
            Id: "590a48f4-d09f-4d9d-b262-d37ce174ef7f",
            Name: "Stade de France",
            Address: "ZAC du Cornillon Nord Saint-Denis, Ile-de-France, France",
            LocationLat: 48.924470,
            LocationLon: 2.360131,
            TimezoneId: "Central European Standard Time",
            UtcOffsetHours: 1,
            Description: string.Empty,
            PhotoImageUrl: string.Empty,
            SeatingPlanImageUrl: string.Empty,
            Halls: new[] {
                new HallContract(
                    Id: "a5540bbb-c153-4f0a-bacc-c23661c0d10e", 
                    Name: "Stadium", 
                    Areas: new[] {
                        new HallAreaContract("67c9bae0-1d3a-4f45-8dea-bef4e6a0ef35", "Gate 1"),
                        new HallAreaContract("7bf3f799-2c69-43c0-a85a-2883a0a4d687", "Gate 2"),
                        new HallAreaContract("126af7ff-f93d-44f6-bb23-b9e7fae63336", "Gate 3"),
                        new HallAreaContract("8bd0d07c-2361-4938-aae2-84a32ae0f088", "Gate 4"),
                    }
                )
            }
        ),
        new VenueContract(
            Id: "0e0f6d58-f8d7-43ed-bac2-4d08efd1613e",
            Name: "Stade Velodrome",
            Address: "3, Boulevard Michelet, 13008 Marseille, Bouches-du-Rhone, France",
            LocationLat: 43.269850,
            LocationLon: 5.395912,
            TimezoneId: "Central European Standard Time",
            UtcOffsetHours: 1,
            Description: string.Empty,
            PhotoImageUrl: string.Empty,
            SeatingPlanImageUrl: string.Empty,
            Halls: new[] {
                new HallContract(
                    Id: "a5540bbb-c153-4f0a-bacc-c23661c0d10e", 
                    Name: "Stadium", 
                    Areas: new[] {
                        new HallAreaContract("59035563-9edf-4a2b-b270-246feb7e0cc0", "A"),
                        new HallAreaContract("6dade2ed-9b32-4246-be73-ff42daa783b3", "B"),
                        new HallAreaContract("a76d86ee-0e5f-497c-9431-aec611114265", "C"),
                        new HallAreaContract("83fc7996-ad93-4834-b6b6-ac2850e8888a", "D"),
                        new HallAreaContract("6b9badf7-f0a8-4e3d-bb90-42f8475e2c71", "E"),
                        new HallAreaContract("76714fa4-8382-48f8-9161-6fee576b6523", "F"),
                    }
                )
            }
        )
    };
}
