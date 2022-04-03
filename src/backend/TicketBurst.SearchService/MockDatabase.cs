using System.Collections.Immutable;
using TicketBurst.Contracts;
using TicketBurst.SearchService.Contracts;

namespace TicketBurst.SearchService;

public static class MockDatabase
{
    public static class Venues
    {
        #region Records
        
        public static readonly VenueContract NeoQuimicaArena = new(
            Id: "ca8eab73-d9ab-4a82-9b81-9f9f32aa9975", 
            Name: "Neo Quimica Arena",
            Address: "Avenida Miguel Ignacio Curi, 111 Sao Paulo, Brazil",
            Location: new GeoPointContract(-23.54525,-46.474278),
            TimeZone: new TimeZoneContract("Brasilia Standard Time", -3),
            Description: string.Empty,
            WebSiteUrl: "https://www.neoquimicaarena.com.br/",
            PhotoImageUrl: string.Empty,
            DefaultCapacity: 49205,
            Halls: MakeStadiumHall(49205)
        ); 

        public static readonly VenueContract EstadioGovernadorMagalhaesPinto = new(
            Id: "0cd69f68-e921-4335-b793-03f361975fdc", 
            Name: "Estadio Governador Magalhaes Pinto",
            Address: "Belo Horizonte, Minas Gerais, Brazil",
            Location: new GeoPointContract(-19.865833, -43.970833),
            TimeZone: new TimeZoneContract("Brasilia Standard Time", -3),
            Description: string.Empty,
            WebSiteUrl: "https://estadiomineirao.com.br/",
            PhotoImageUrl: string.Empty,
            DefaultCapacity: 61846,
            Halls: MakeStadiumHall(61846)
        ); 

        public static readonly VenueContract ItaipavaArenaFonteNova = new(
            Id: "d5540ce3-46fc-41b8-9e1c-76b9017b302a", 
            Name: "Itaipava Arena Fonte Nova",
            Address: "Ladeira da Fonte das Pedras, Nazare, Salvador, Brazil",
            Location: new GeoPointContract(-12.978611, -38.504167),
            TimeZone: new TimeZoneContract("Brasilia Standard Time", -3),
            Description: string.Empty,
            WebSiteUrl: "https://www.itaipavaarenafontenova.com.br/",
            PhotoImageUrl: string.Empty,
            DefaultCapacity: 48000,
            Halls: MakeStadiumHall(48000)
        ); 

        public static readonly VenueContract ArenaBRBManeGarrincha = new(
            Id: "1dcad226-6fcf-43d9-8eaf-04990860664c", 
            Name: "Arena BRB Mane Garrincha",
            Address: "SRPN Estadio Nacional Mane Garrincha Brasília, Distrito Federal, Brazil",
            Location: new GeoPointContract(-15.783626, -47.899050),
            TimeZone: new TimeZoneContract("Brasilia Standard Time", -3),
            Description: string.Empty,
            WebSiteUrl: "https://arenabsb.com.br/",
            PhotoImageUrl: string.Empty,
            DefaultCapacity: 72788,
            Halls: MakeStadiumHall(72788)
        ); 

        public static readonly VenueContract EstadioJornalistaMarioFilho = new(
            Id: "b676fad0-9323-4b72-b250-66fc48110fda", 
            Name: "Estadio Jornalista Mario Filho",
            Address: "Maracana, Rio de Janeiro, Brazil",
            Location: new GeoPointContract(-22.912173, -43.230177),
            TimeZone: new TimeZoneContract("Brasilia Standard Time", -3),
            Description: string.Empty,
            WebSiteUrl: "https://www.estadiodomaracana.com.br/",
            PhotoImageUrl: string.Empty,
            DefaultCapacity: 78838,
            Halls: MakeStadiumHall(78838)
        ); 

        #endregion

        public static readonly ImmutableList<VenueContract> All = MakeImmutableList(
            #region Items
            
            NeoQuimicaArena,
            EstadioGovernadorMagalhaesPinto,
            ItaipavaArenaFonteNova,
            ArenaBRBManeGarrincha,
            EstadioJornalistaMarioFilho
            
            #endregion
        );
    }

    public static class Genres
    {
        public static readonly GenreContract Football = new(
            Id: "28a28fa5-1e95-4810-b9c6-b52dc8a93d29", 
            Name: "Football", 
            PosterImageUrl: string.Empty); 

        public static readonly ImmutableList<GenreContract> All = MakeImmutableList(Football);
    }

    public static class ShowTypes
    {
        public static ShowTypeContract Match = new(
            Id: "180bac07-f03a-4c3c-994b-d3ee9926428f",
            Name: "Match",
            PosterImageUrl: string.Empty);
        
        public static readonly ImmutableList<ShowTypeContract> All = MakeImmutableList(
            Match
        );
    }

    public static class Shows
    {
        public static class Football
        {
            #region Records
            
            public static readonly ShowContract QuarterFinal = new(
                Id: "8f75f4e7-7391-4158-8db9-e370c0bfe2e3",
                ShowTypeId: ShowTypes.Match.Id,
                GenreId: Genres.Football.Id,
                TroupeIds: null,
                Title: "1/4 Final",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly ShowContract SemiFinal = new(
                Id: "a598be4d-05b6-435b-818a-81bcbba9a509",
                ShowTypeId: ShowTypes.Match.Id,
                GenreId: Genres.Football.Id,
                TroupeIds: null,
                Title: "1/2 Final",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly ShowContract GoldMedalFinal = new(
                Id: "d34daf55-0daa-4273-b1b4-95cc84d340eb",
                ShowTypeId: ShowTypes.Match.Id,
                GenreId: Genres.Football.Id,
                TroupeIds: null,
                Title: "Gold Medal Final",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly ShowContract BronzeMedalFinal = new(
                Id: "7ff48d6a-9d7e-4e38-a458-043ea2e9ab9d",
                ShowTypeId: ShowTypes.Match.Id,
                GenreId: Genres.Football.Id,
                TroupeIds: null,
                Title: "Bronze Medal Final",
                Description: string.Empty,
                PosterImageUrl: string.Empty);
            
            #endregion
        }
        
        public static readonly ImmutableList<ShowContract> All = MakeImmutableList(
            #region Items
            
            Football.QuarterFinal,
            Football.SemiFinal,
            Football.GoldMedalFinal,
            Football.BronzeMedalFinal
            
            #endregion
        );
    }


    public static class Troupes
    {
        public static class Football
        {
            #region Records
            
            public static TroupeContract Brazil = new(
                Id: "f4dbdf83-bcc2-473d-80a9-c911e6ffef5f",
                Name: "Brazil",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);

            public static TroupeContract Colombia = new(
                Id: "5eb2c786-899a-4639-9073-5f9a491f17b5",
                Name: "Colombia",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);
            
            public static TroupeContract SouthKorea = new(
                Id: "9c8aa818-aaa3-4b21-a736-8b5c4cdff294",
                Name: "South Korea",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);
                        
            public static TroupeContract Honduras = new(
                Id: "d9e8a382-7d6b-4517-b07d-757d5b1f00cb",
                Name: "Honduras",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);

            public static TroupeContract Nigeria = new(
                Id: "659e0c93-7ec6-415a-9fde-6396b2297c15",
                Name: "Nigeria",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);

            public static TroupeContract Denmark = new(
                Id: "7e5339a0-1365-441e-bee3-e7532f4c13b3",
                Name: "Denmark",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);

            public static TroupeContract Portugal = new(
                Id: "bfc01344-169e-470f-9318-8cd923192529",
                Name: "Portugal",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);

            public static TroupeContract Germany = new(
                Id: "45ca7593-86b9-4ee4-bcb7-fa5fcffe1952",
                Name: "Germany",
                PosterImageUrl: string.Empty,
                GenreId: Genres.Football.Id);
            
            #endregion
        }
    
        public static readonly ImmutableList<TroupeContract> All = MakeImmutableList(
            #region Items
            
            Football.Brazil,
            Football.Colombia,
            Football.SouthKorea,
            Football.Nigeria,
            Football.Denmark,
            Football.Honduras,
            Football.Portugal,
            Football.Germany
            
            #endregion
        );
    }

    public static class HallSeatingMaps
    {
        public static ImmutableList<HallSeatingMapContract> All = ImmutableList<HallSeatingMapContract>.Empty;

        public static void Add(HallSeatingMapContract map)
        {
            All = All.Add(map);
        }
    }
    

    public static class Events
    {
        public static readonly TimeSpan TimeToOpenSale = TimeSpan.FromSeconds(10);
        
        public static class Football
        {
            public static readonly EventContract QuarterFinal1of4 = new(
                Id: "d8deb62d-6ada-410c-9016-45c0480d06cc",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.QuarterFinal.Id,
                TroupeIds: MakeImmutableList(Troupes.Football.Brazil.Id, Troupes.Football.Colombia.Id),
                VenueId: Venues.NeoQuimicaArena.Id,
                HallId: Venues.NeoQuimicaArena.Halls[0].Id,
                HallSeatingMapId: Venues.NeoQuimicaArena.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 12, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.NeoQuimicaArena),
                IsOpenForSale: false,
                Title: $"{Troupes.Football.Brazil.Name} - {Troupes.Football.Colombia.Name}",
                Description: string.Empty,
                PosterImageUrl: string.Empty);
            
            public static readonly EventContract QuarterFinal2of4 = new(
                Id: "d22508dd-dec2-4c25-b209-007bcdd3ae02",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.QuarterFinal.Id,
                TroupeIds: MakeImmutableList(Troupes.Football.SouthKorea.Id, Troupes.Football.Honduras.Id),
                VenueId: Venues.EstadioGovernadorMagalhaesPinto.Id,
                HallId: Venues.EstadioGovernadorMagalhaesPinto.Halls[0].Id,
                HallSeatingMapId: Venues.EstadioGovernadorMagalhaesPinto.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 12, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.EstadioGovernadorMagalhaesPinto),
                IsOpenForSale: false,
                Title: $"{Troupes.Football.SouthKorea.Name} - {Troupes.Football.Honduras.Name}",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract QuarterFinal3of4 = new(
                Id: "a8fa9ef4-d020-4512-8ad8-7c897258dae7",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.QuarterFinal.Id,
                TroupeIds: MakeImmutableList(Troupes.Football.Nigeria.Id, Troupes.Football.Denmark.Id),
                VenueId: Venues.ItaipavaArenaFonteNova.Id,
                HallId: Venues.ItaipavaArenaFonteNova.Halls[0].Id,
                HallSeatingMapId: Venues.ItaipavaArenaFonteNova.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 12, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.ItaipavaArenaFonteNova),
                IsOpenForSale: false,
                Title: $"{Troupes.Football.Nigeria.Name} - {Troupes.Football.Denmark.Name}",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract QuarterFinal4of4 = new(
                Id: "b528b676-a7c7-4141-be85-a2cb3535e2d7",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.QuarterFinal.Id,
                TroupeIds: MakeImmutableList(Troupes.Football.Portugal.Id, Troupes.Football.Germany.Id),
                VenueId: Venues.ArenaBRBManeGarrincha.Id,
                HallId: Venues.ArenaBRBManeGarrincha.Halls[0].Id,
                HallSeatingMapId: Venues.ArenaBRBManeGarrincha.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 12, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.ArenaBRBManeGarrincha),
                IsOpenForSale: false,
                Title: $"{Troupes.Football.Portugal.Name} - {Troupes.Football.Germany.Name}",
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract SemiFinal1of2 = new(
                Id: "67e0d9e7-d062-427d-a6cd-020e791ad0c7",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.SemiFinal.Id,
                TroupeIds: null,
                VenueId: Venues.EstadioJornalistaMarioFilho.Id,
                HallId: Venues.EstadioJornalistaMarioFilho.Halls[0].Id,
                HallSeatingMapId: Venues.EstadioJornalistaMarioFilho.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 15, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.EstadioJornalistaMarioFilho),
                IsOpenForSale: false,
                Title: string.Empty,
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract SemiFinal2of2 = new(
                Id: "d27e1604-8122-48ed-abf9-feaf9f677000",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.SemiFinal.Id,
                TroupeIds: null,
                VenueId: Venues.NeoQuimicaArena.Id,
                HallId: Venues.NeoQuimicaArena.Halls[0].Id,
                HallSeatingMapId: Venues.NeoQuimicaArena.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 15, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.NeoQuimicaArena),
                IsOpenForSale: false,
                Title: string.Empty,
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract GoldMedalFinal = new(
                Id: "bfa81ffc-aebb-4514-8ed6-a85dc44aaab1",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.GoldMedalFinal.Id,
                TroupeIds: null,
                VenueId: Venues.EstadioJornalistaMarioFilho.Id,
                HallId: Venues.EstadioJornalistaMarioFilho.Halls[0].Id,
                HallSeatingMapId: Venues.EstadioJornalistaMarioFilho.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 20, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.EstadioJornalistaMarioFilho),
                IsOpenForSale: false,
                Title: string.Empty,
                Description: string.Empty,
                PosterImageUrl: string.Empty);

            public static readonly EventContract BronzeMedalFinal = new(
                Id: "fb2cf88d-8c46-4e79-a085-ce30bb78c4c9",
                ShowTypeId: ShowTypes.Match.Id,
                ShowId: Shows.Football.BronzeMedalFinal.Id,
                TroupeIds: null,
                VenueId: Venues.EstadioGovernadorMagalhaesPinto.Id,
                HallId: Venues.EstadioGovernadorMagalhaesPinto.Halls[0].Id,
                HallSeatingMapId: Venues.EstadioGovernadorMagalhaesPinto.Halls[0].DefaultSeatingMapId,
                SaleStartUtc: DateTime.UtcNow.Add(TimeToOpenSale),
                EventStartUtc: new DateTime(2022, 4, 20, 10, 30, 0, DateTimeKind.Local),
                DurationMinutes: 180,
                PriceList: MakePriceList(Venues.EstadioGovernadorMagalhaesPinto),
                IsOpenForSale: false,
                Title: string.Empty,
                Description: string.Empty,
                PosterImageUrl: string.Empty);
        }

        private static ImmutableList<EventContract> __all = MakeImmutableList(
            #region Items
            
            Football.QuarterFinal1of4,
            Football.QuarterFinal2of4,
            Football.QuarterFinal3of4,
            Football.QuarterFinal4of4,
            Football.SemiFinal1of2,
            Football.SemiFinal2of2,
            Football.GoldMedalFinal,
            Football.BronzeMedalFinal

            #endregion
        );

        public static ImmutableList<EventContract> All => __all;

        public static void UpdateIsOpenForSale(string eventId, bool newValue)
        {
            __all = __all
                .Select(e => e.Id != eventId
                    ? e
                    : e with { IsOpenForSale = newValue })
                .ToImmutableList();
        }

        private static EventPriceListContract MakePriceList(VenueContract venue)
        {
            var hall = venue.Halls[0];
            var hallSeatingMap = HallSeatingMaps.All.First(map => map.Id == hall.DefaultSeatingMapId);
            var priceLevels = hallSeatingMap.PriceLevels;
            var price = (decimal)__random.Next(50, 100);
            var builder = ImmutableDictionary.CreateBuilder<string, decimal>();

            for (int i = 0; i < priceLevels.Count; i++)
            {
                builder[priceLevels[i].Id] = price;
                price += __random.Next(50, 100);
            }

            return new EventPriceListContract(PriceByLevelId: builder.ToImmutable());
        }
    }


    private static ImmutableList<HallContract> MakeStadiumHall(int hallCapacity)
    {
        var eachAreaCapacity = 500;
        SplitCapacityIntoBoxes(hallCapacity, eachAreaCapacity, out var areaCount, out var lastAreaCapacity);

        var hallId = MakeNewId();
        var areaIds = Enumerable.Range(1, areaCount).Select(_ => MakeNewId()).ToArray();

        var seatingMap = CreateHallSeatingMap();
        var hall = new HallContract(
            Id: hallId,
            Name: "Stadium",
            SeatingPlanImageUrl: string.Empty,
            Areas: Enumerable.Range(0, areaCount).Select(areaIndex => new HallAreaContract(
                Id: areaIds[areaIndex],
                Name: (areaIndex + 1).ToString(),
                SeatingPlanImageUrl: string.Empty
            )).ToImmutableList(),
            DefaultSeatingMapId: seatingMap.Id 
        );
        
        HallSeatingMaps.Add(seatingMap);
        return ImmutableList<HallContract>.Empty.Add(hall);

        HallSeatingMapContract CreateHallSeatingMap()
        {
            var seatingMapId = MakeNewId();
            var priceLevels = MakePriceLevels();
            return new HallSeatingMapContract(
                Id: seatingMapId,
                HallId: hallId,
                Name: "Default",
                Capacity: hallCapacity,
                PlanImageUrl: string.Empty,
                PriceLevels: priceLevels,
                Areas: areaIds.Select((areaId, areaIndex) => CreateAreaSeatingMap(
                    seatingMapId,
                    areaId,
                    areaName: (areaIndex + 1).ToString(),
                    areaCapacity: areaId == areaIds[^1] ? lastAreaCapacity : eachAreaCapacity,
                    priceLevelIds: PickPriceLevelIds(priceLevels)
                )).ToImmutableList()
            );
        }

        AreaSeatingMapContract CreateAreaSeatingMap(
            string seatingMapId, 
            string areaId, 
            string areaName, 
            int areaCapacity,
            string[] priceLevelIds)
        {
            var seatsInEachRow = 25;
            SplitCapacityIntoBoxes(areaCapacity, seatsInEachRow, out var rowCount, out var seatsInLastRow);

            var midRowNum = rowCount / priceLevelIds.Length;
            var priceLevelQuadrants = MakePriceLevelQuadrants(priceLevelIds);
            
            return new AreaSeatingMapContract(
                SeatingMapId: seatingMapId,
                HallAreaId: areaId,
                HallAreaName: areaName,
                Capacity: areaCapacity,
                PlanImageUrl: string.Empty,
                Rows: Enumerable.Range(1, rowCount).Select(rowNum => {
                    var numSeatsInThisRow = rowNum < rowCount ? seatsInEachRow : seatsInLastRow;
                    return CreateAreaSeatingMapRow(
                        rowNum,
                        numSeatsInThisRow,
                        ConcatPriceLevelIds(rowNum, midRowNum, numSeatsInThisRow, priceLevelQuadrants)
                    );
                }).ToImmutableList() 
            );
        }

        string[] PickPriceLevelIds(ImmutableList<PriceLevelContract> priceLevels)
        {
            var firstIndex = __random.Next(100) % priceLevels.Count;
            if (__random.Next(100) < 30)
            {
                return new[] { priceLevels[firstIndex].Id };
            }

            var secondIndex = (firstIndex + 1) % priceLevels.Count;
            return new[] {
                priceLevels[firstIndex].Id,
                priceLevels[secondIndex].Id
            };
        }
        
        string[,] MakePriceLevelQuadrants(string[] priceLevelIds)
        {
            var dice = __random.Next(33);
            if (priceLevelIds.Length == 1 || dice < 10)
            {
                return new string[,] {
                    { priceLevelIds[0], priceLevelIds[0], priceLevelIds[0] },
                    { priceLevelIds[0], priceLevelIds[0], priceLevelIds[0] },
                };
            }
            else if (dice < 19)
            {
                return new string[,] {
                    { priceLevelIds[0], priceLevelIds[0], priceLevelIds[0] },
                    { priceLevelIds[1], priceLevelIds[1], priceLevelIds[1] },
                };
            }
            else 
            {
                return new string[,] {
                    { priceLevelIds[1], priceLevelIds[0], priceLevelIds[1] },
                    { priceLevelIds[1], priceLevelIds[1], priceLevelIds[1] },
                };
            }
        }

        IEnumerable<string> ConcatPriceLevelIds(int rowIndex, int midRowIndex, int numSeatsInRow, string[,] quadrants)
        {
            var quadrantRow = rowIndex < midRowIndex ? 0 : 1;
            return Enumerable.Repeat(quadrants[quadrantRow, 0], 5)
                .Concat(Enumerable.Repeat(quadrants[quadrantRow, 1], numSeatsInRow - 10))
                .Concat(Enumerable.Repeat(quadrants[quadrantRow, 2], 5));
        }

        SeatingMapRowContract CreateAreaSeatingMapRow(int rowNum, int seatCount, IEnumerable<string> priceLevelIds)
        {
            using var priceLevelIdEnumerator = priceLevelIds.GetEnumerator();
            
            return new SeatingMapRowContract(
                Id: MakeNewId(),
                Name: rowNum.ToString(),
                Seats: Enumerable.Range(1, seatCount).Select(seatNum => new SeatingMapSeatContract(
                    Id: MakeNewId(),
                    Name: seatNum.ToString(),
                    // ReSharper disable once AccessToDisposedClosure
                    PriceLevelId: TakeNext(priceLevelIdEnumerator)
                )).ToImmutableList()
            );
        }

        T TakeNext<T>(IEnumerator<T> enumerator)
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }

        static void SplitCapacityIntoBoxes(
            int totalCapacity, 
            int eachBoxCapacity, 
            out int boxCount,
            out int lastBoxCapacity)
        {
            boxCount = totalCapacity / eachBoxCapacity;
            lastBoxCapacity = (totalCapacity % eachBoxCapacity) != 0
                ? eachBoxCapacity + (totalCapacity % eachBoxCapacity)
                : eachBoxCapacity;
        }

        static ImmutableList<PriceLevelContract> MakePriceLevels()
        {
            return new[] {
                new PriceLevelContract(MakeNewId(), "A", "0693e3"),
                new PriceLevelContract(MakeNewId(), "B", "fcb900"),
                new PriceLevelContract(MakeNewId(), "C", "9b51e0"),
            }.ToImmutableList();
        }
    }

    private static readonly Random __random = new Random(123456);
    
    public static string MakeNewId()
    {
        return Guid.NewGuid().ToString("d");
    }

    private static ImmutableList<T> MakeImmutableList<T>(params T[] items)
    {
        return ImmutableList<T>.Empty.AddRange(items);
    }
}

