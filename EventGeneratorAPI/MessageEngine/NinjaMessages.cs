using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventGeneratorAPI.MessageEngine
{
    static partial class Messages
    {
        private static readonly dynamic ninjaAttack;

        static Messages() {
            if (Environment.GetEnvironmentVariable("isLocal") == "1")
            {
                ninjaAttack = JObject.Parse(File.ReadAllText(System.IO.Path.GetFullPath(@"NinjaAttack.json")));
            } else
            {
                ninjaAttack = JObject.Parse(File.ReadAllText(Path.Combine(Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process), @"site\wwwroot\NinjaAttack.json")));
            }
        }

        internal static string[] CreateNinjaMessages(int numOfMessages)
        {

            string[] messages = new string[numOfMessages];
            Random random = new Random();

            for (int i = 0; i < numOfMessages; i++)
            {
                //decide if the good or bad ninja will be the actor and pick which good and bad ninjas will be involved
                bool goodNinjaActor = random.Next(1) == 1 ? true : false;
                int goodNinjaIndex = random.Next(ninjaAttack.goodNinjas.Count);
                int badNinjaIndex = random.Next(ninjaAttack.badNinjas.Count);
                string actor = goodNinjaActor ? (string)ninjaAttack.goodNinjas[goodNinjaIndex] : (string)ninjaAttack.badNinjas[badNinjaIndex];
                //whoever isn't the actor is the actee
                string target = goodNinjaActor ? (string)ninjaAttack.badNinjas[badNinjaIndex] : (string)ninjaAttack.goodNinjas[badNinjaIndex];

                //get the index of the weapon to be used so we can grab the properties
                int weaponIndex = random.Next(ninjaAttack.weapons.Count);
                string weapon = ninjaAttack.weapons[weaponIndex].name;

                //get the index of the action for the weapon to be used so we can grab the properties
                int actionIndex = random.Next(ninjaAttack.weapons[weaponIndex].actions.Count);
                string action = ninjaAttack.weapons[weaponIndex].actions[actionIndex].name;
                int points = ninjaAttack.weapons[weaponIndex].actions[actionIndex].points;

                var data = new
                {
                    actor,
                    side = goodNinjaActor ? "good" : "bad",
                    weapon,
                    action,
                    target,
                    points,
                    description = $"Message {i}: {actor} {action} {target} with {weapon} for {points} points"
                };

                messages[i] = JsonConvert.SerializeObject(data);
            }
            return messages;
        }
    }
}
