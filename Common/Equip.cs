using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class PropaneLevel
    {
        public PropaneLevel (double levelPct, DateTime readingTime, double outsideTempF)
        {
            LevelPct = levelPct;
            ReadingTime = readingTime;
            OutsideTempF = outsideTempF;
        }

        public double OutsideTempF = -40;
        public double LevelPct = 0;
        public DateTime ReadingTime;
    }
}
