using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
    
    public enum SchemeMode
    {
        // Початковий стан, все вимкнено/стандартно
        Idle,

        // Режим "А" (наприклад, потік іде по трубі 1)
        ModeA_Flow,

        // Режим "Б" (потік по трубі 2)
        ModeB_Flow,

        // Якась помилка
        Failure_Leak
    }
}
