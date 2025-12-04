using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Stand7
{
    public class PipeViewModel:BaseViewModel
    {
        public int Id { get; set; } // Просто щоб ми їх розрізняли

        // Координати для прив'язки в XAML
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        // --- ГОЛОВНЕ ---
        // Кожна труба сама знає свій колір!
        private Brush _color;
        public Brush Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged(); // 3. Повідомляй View про зміни!
            }
        }
    }
}
