using System;
using System.Collections.Generic;
using System.Dynamic;
using Net.Code.Csv;

namespace Retrosheet.Utilities.GameLogUtility
{
    public class GameLogFactory
    {
        private readonly string[] _properties;

        public GameLogFactory(string[] properties)
        {
            _properties = properties;
        }

        public ExpandoObject CreateGameLogFromCsv(string csv)
        {
            var gameLog = new ExpandoObject();

            using (var csvReader = ReadCsv.FromString(csv))
            {
                while (csvReader.Read())
                {
                    for (var i = 0; i < _properties.Length; i++)
                    {
                        var added = gameLog.TryAdd(_properties[i], csvReader[i]);

                        if (!added)
                        {
                            throw new InvalidOperationException($"The property {_properties[i]} could not be added.");
                        }
                    }

                    var dyno = ((dynamic) gameLog);
                    var gameDate = dyno.game_date;
                    var gameYear = (string)gameDate.Substring(0, 4);
                    var gameMonth = (string)gameDate.Substring(4, 2);
                    var gameDay = (string)gameDate.Substring(6, 2);
                    
                    gameLog.TryAdd("game_year", short.Parse(gameYear));
                    gameLog.TryAdd("game_month", byte.Parse(gameMonth));
                    gameLog.TryAdd("game_day", byte.Parse(gameDay));
                }

            }
            
            return gameLog;
        }
    }
}