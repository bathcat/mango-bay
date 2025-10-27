using System;
using System.Collections.Generic;
using MBC.Core.Entities;

namespace MBC.Services.Seeds.Data;

public static class Pilots
{
    public static IEnumerable<Pilot> Seeds =>
    [
        new Pilot
        {
            Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            FullName = "Baloo",
            ShortName = "baloo",
            AvatarUrl = "pilot-avatars/baloo.webp",
            Bio = "Baloo is a seasoned cargo pilot with years of experience flying across the tropical islands. His laid-back approach and natural piloting instincts make even the toughest deliveries look easy. When he's not in the air, you'll find him enjoying a cold drink and swapping stories at the local watering hole. Customers love his easygoing charm and his knack for getting any cargo delivered, no matter the weather or the deadline.",
            UserId = new Guid("a0000000-0000-0000-0000-000000000001")
        },
        new Pilot
        {
            Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            FullName = "Mowgli",
            ShortName = "mowgli",
            AvatarUrl = "pilot-avatars/mowgli.webp",
            Bio = "Mowgli is our youngest navigator and brings fresh energy to every flight. Trained under Baloo's expert guidance, he combines youthful enthusiasm with surprising skill and reliability. His innovative flying techniques have become legendary around the tropical islands. Mowgli's dedication to learning and his fearless attitude make him a rising star in the cargo aviation world.",
            UserId = new Guid("a0000000-0000-0000-0000-000000000002")
        },
        new Pilot
        {
            Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            FullName = "Raksha",
            ShortName = "raksha",
            AvatarUrl = "pilot-avatars/raksha.webp",
            Bio = "Raksha brings business savvy and exceptional piloting skills to every delivery. As both an accomplished pilot and operations expert, she runs her flights with precision and professionalism. Her attention to detail and commitment to customer satisfaction have earned her a stellar reputation throughout the tropical islands. Raksha's flights always arrive on time and on budget, making her the go-to choice for business-critical shipments.",
            UserId = new Guid("a0000000-0000-0000-0000-000000000003")
        },
        new Pilot
        {
            Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            FullName = "Hathi",
            ShortName = "hathi",
            AvatarUrl = "pilot-avatars/hathi.webp",
            Bio = "Hathi is a mechanical genius whose unconventional methods somehow always work. While his approach might seem unorthodox, his deep understanding of aircraft keeps our fleet flying in top condition. He combines piloting with an innate ability to fix any mechanical issue mid-flight. Customers appreciate his can-do attitude and his ability to deliver cargo even when equipment complications arise.",
            UserId = new Guid("a0000000-0000-0000-0000-000000000004")
        },
        new Pilot
        {
            Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            FullName = "Shanti",
            ShortName = "shanti",
            AvatarUrl = "pilot-avatars/shanti.jpg",
            Bio = "Shanti is our most enthusiastic pilot-in-training, bringing boundless energy and fearless determination to every flight. Despite her youth, she has already demonstrated impressive natural flying ability and quick thinking in challenging situations. Her infectious optimism and eagerness to learn make her a favorite among both crew members and customers. Watch the skies for this rising aviation star.",
            UserId = new Guid("a0000000-0000-0000-0000-000000000005")
        }
    ];
}
