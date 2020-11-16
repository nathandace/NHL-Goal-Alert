using System;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NHLGoalAlert
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();
        private static int _currentScore;

        static async Task Main(string[] args)
        {
            var powerPlayStarTime = DateTime.Now.AddDays(-1);

            while (true)
            {
                Console.Write($"\r{DateTime.Now}");

                var stringTask = await Client.GetStringAsync("https://statsapi.web.nhl.com/api/v1/schedule?teamId=19&expand=schedule.linescore");
                var json = JObject.Parse(stringTask);

                var date = DateTime.Parse((string) json["dates"][0]["date"]);

                if (date.Date == DateTime.Now.Date)
                {
                    var homeOrAway = (string) json["dates"][0]["games"][0]["linescore"]["teams"]["home"]["team"]["id"] == "19" ? "home" : "away";
                    var score = int.Parse((string) json["dates"][0]["games"][0]["linescore"]["teams"][homeOrAway]["goals"]);

                    var powerPlay = bool.Parse((string) json["dates"][0]["games"][0]["linescore"]["teams"][homeOrAway]["powerPlay"]);
                    
                    if (powerPlay)
                    {
                        var now = DateTime.Now;

                        if (now >= powerPlayStarTime.AddMinutes(10))
                        {
                            powerPlayStarTime = DateTime.Now;

                            using (var powerPlaySound = new SoundPlayer {SoundLocation = Environment.CurrentDirectory + "/blues_powerplay.wav"})
                            {
                                powerPlaySound.PlaySync();
                            }
                        }
                    }

                    if (score > _currentScore)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));

                        using (var goalSound = new SoundPlayer {SoundLocation = Environment.CurrentDirectory + "/blues.wav"})
                        {
                            goalSound.PlaySync();
                        }

                        _currentScore = score;
                    }
                }

                var delay = TimeSpan.FromSeconds(1);
                await Task.Delay(delay);
            }
        }
    }
}
