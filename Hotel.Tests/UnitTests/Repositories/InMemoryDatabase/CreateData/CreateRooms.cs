﻿using Hotel.Domain.Entities.ReservationEntity;
using Hotel.Domain.Entities.RoomEntity;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Tests.UnitTests.Repositories.Mock.CreateData;

public class CreateRooms
{
    public static async Task Create()
    {
        var rooms = new List<Room>()
        {
            new("Um quarto 1",4, 50m, 3, "Um quarto para hospedagem.", BaseRepositoryTest.Categories[1], ""),
            new("Um quarto 2",3, 110m, 4, "Um quarto com vista para a praia.", BaseRepositoryTest.Categories[0], ""),
            new("Quarto Standard 1",9, 50m, 3, "Quarto Standard", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Deluxe 1",11, 90m, 2, "Suíte Deluxe", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Executiva 2",13, 110m, 2, "Suíte Executiva", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Familiar 3",29, 70m, 4, "Quarto Familiar", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Presidencial 4",14, 150m, 2, "Suíte Presidencial", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Standard 5",12, 80m, 2, "Quarto Standard", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Familiar 6",21, 120m, 3, "Suíte Familiar", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Real 7",25, 200m, 2, "Suíte Real", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Deluxe 8 ",19, 100m, 2, "Quarto Deluxe", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Presidencial 9",31, 180m, 2, "Suíte Presidencial", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Presidencial 10",32, 300m, 3, "Quarto Presidencial V2", BaseRepositoryTest.Categories[4], ""),

        }.OrderBy(x => x.Number).ToList();

        new Reservation(rooms[12], DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), BaseRepositoryTest.Customers[0], 2, "");

        BaseRepositoryTest.AvailableRooms = new List<Room>
        {
            new("Quarto Conforto 11",55, 60m, 3, "Quarto Conforto", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Luxo 12",56, 105m, 2, "Suíte Luxo", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Executiva 13",57, 125m, 2, "Suíte Executiva Premium", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Família 14",58, 85m, 4, "Quarto Família Plus", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Presidencial 15",59, 165m, 2, "Suíte Presidencial Luxo", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Econômico 16",60, 95m, 2, "Quarto Econômico", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Master 17",61, 135m, 3, "Suíte Master", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Imperial 18",62, 215m, 2, "Suíte Imperial", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Superior 19",63, 115m, 2, "Quarto Superior", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Presidencial 20",64, 195m, 2, "Suíte Presidencial Premium", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Imperial 21",65, 330m, 3, "Quarto Imperial V2", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Conforto 22",66, 65m, 3, "Quarto Conforto Plus", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Luxo 23",67, 110m, 2, "Suíte Luxo Premium", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Executiva 24",68, 130m, 2, "Suíte Executiva Superior", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Família 25",69, 90m, 4, "Quarto Família Deluxe", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Presidencial 26",70, 170m, 2, "Suíte Presidencial Elite", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Econômico 27",71, 100m, 2, "Quarto Econômico Plus", BaseRepositoryTest.Categories[1], ""),
            new("Suíte Master 28",72, 140m, 3, "Suíte Master Deluxe", BaseRepositoryTest.Categories[3], ""),
            new("Suíte Imperial 29",73, 220m, 2, "Suíte Imperial Luxo", BaseRepositoryTest.Categories[2], ""),
            new("Quarto Superior 30",74, 120m, 2, "Quarto Superior Plus", BaseRepositoryTest.Categories[0], ""),
            new("Suíte Presidencial 31",75, 200m, 2, "Suíte Presidencial Superior", BaseRepositoryTest.Categories[4], ""),
            new("Quarto Imperial 32",76, 340m, 3, "Quarto Imperial V3", BaseRepositoryTest.Categories[4], ""),
        };

        await BaseRepositoryTest.MockConnection.Context.Rooms.AddRangeAsync(rooms.Concat(BaseRepositoryTest.AvailableRooms));
        await BaseRepositoryTest.MockConnection.Context.SaveChangesAsync();

        BaseRepositoryTest.Rooms = await BaseRepositoryTest.MockConnection.Context.Rooms.ToListAsync();

        BaseRepositoryTest.Rooms[0].AddService(BaseRepositoryTest.Services[0]);
        BaseRepositoryTest.Rooms[0].AddService(BaseRepositoryTest.Services[2]);
        BaseRepositoryTest.Rooms[1].AddService(BaseRepositoryTest.Services[4]);
        BaseRepositoryTest.Rooms[3].AddService(BaseRepositoryTest.Services[1]);
        BaseRepositoryTest.Rooms[4].AddService(BaseRepositoryTest.Services[3]);
        BaseRepositoryTest.Rooms[4].AddService(BaseRepositoryTest.Services[0]);
        BaseRepositoryTest.Rooms[6].AddService(BaseRepositoryTest.Services[2]);
        BaseRepositoryTest.Rooms[8].AddService(BaseRepositoryTest.Services[4]);
        BaseRepositoryTest.Rooms[10].AddService(BaseRepositoryTest.Services[1]);
        BaseRepositoryTest.Rooms[11].AddService(BaseRepositoryTest.Services[3]);
        BaseRepositoryTest.Rooms[11].AddService(BaseRepositoryTest.Services[0]);
        BaseRepositoryTest.Rooms[1].AddService(BaseRepositoryTest.Services[2]);
        BaseRepositoryTest.Rooms[5].AddService(BaseRepositoryTest.Services[3]);
        BaseRepositoryTest.Rooms[7].AddService(BaseRepositoryTest.Services[1]);
        BaseRepositoryTest.Rooms[9].AddService(BaseRepositoryTest.Services[0]);

        await BaseRepositoryTest.MockConnection.Context.SaveChangesAsync();
        BaseRepositoryTest.Rooms = await BaseRepositoryTest.MockConnection.Context.Rooms.OrderBy(x => x.Number).ToListAsync();
    }
}
