using System;

namespace ResponsiJuniorProject.Models
{
    /// <summary>
    /// ABSTRACT class Developer - demonstrates ABSTRACTION in OOP
    /// Kelas ini tidak bisa di-instantiate langsung, harus melalui derived class
    /// </summary>
    public abstract class Developer : BaseEntity
    {
        // Protected fields - encapsulation (accessible by derived classes)
        protected int _idProyek;
        protected string _namaDev;
        protected string _statusKontrak;
        protected int _fiturSelesai;
        protected int _jumlahBug;

        // Konstanta gaji pokok untuk Full Time
        protected const decimal GAJI_POKOK_FULLTIME = 5000000.00m;

        #region Properties with Encapsulation

        public int IdDev
        {
            get { return _id; }
            set { _id = value; }
        }

        public int IdProyek
        {
            get { return _idProyek; }
            set { _idProyek = value; }
        }

        public string NamaDev
        {
            get { return _namaDev; }
            set { _namaDev = value; }
        }

        public string StatusKontrak
        {
            get { return _statusKontrak; }
            set { _statusKontrak = value; }
        }

        public int FiturSelesai
        {
            get { return _fiturSelesai; }
            set
            {
                if (value >= 0)
                    _fiturSelesai = value;
                else
                    throw new ArgumentException("Fitur selesai tidak boleh negatif");
            }
        }

        public int JumlahBug
        {
            get { return _jumlahBug; }
            set
            {
                if (value >= 0)
                    _jumlahBug = value;
                else
                    throw new ArgumentException("Jumlah bug tidak boleh negatif");
            }
        }

        #endregion

        // Default constructor
        protected Developer()
        {
        }

        /// <summary>
        /// ABSTRACT method - HARUS diimplementasikan oleh derived class (POLYMORPHISM)
        /// Menghitung skor performa developer
        /// </summary>
        public abstract double HitungSkor();

        /// <summary>
        /// ABSTRACT method - HARUS diimplementasikan oleh derived class (POLYMORPHISM)
        /// Menghitung total gaji developer
        /// </summary>
        public abstract decimal HitungGaji();

        // Override abstract method from base class
        public override string GetInfo()
        {
            return $"Developer: {NamaDev}, Status: {StatusKontrak}, Skor: {HitungSkor():F2}, Gaji: Rp {HitungGaji():N0}";
        }

        // Override ToString
        public override string ToString()
        {
            return NamaDev;
        }

        /// <summary>
        /// Factory method - creates appropriate Developer subclass based on status
        /// </summary>
        public static Developer Create(string statusKontrak)
        {
            if (statusKontrak == "Full Time")
                return new DeveloperFullTime();
            else
                return new DeveloperFreelance();
        }

        /// <summary>
        /// Factory method with all parameters
        /// </summary>
        public static Developer Create(int idDev, int idProyek, string namaDev, string statusKontrak, int fiturSelesai, int jumlahBug)
        {
            Developer dev;
            if (statusKontrak == "Full Time")
                dev = new DeveloperFullTime();
            else
                dev = new DeveloperFreelance();

            dev.IdDev = idDev;
            dev.IdProyek = idProyek;
            dev.NamaDev = namaDev;
            dev.StatusKontrak = statusKontrak;
            dev.FiturSelesai = fiturSelesai;
            dev.JumlahBug = jumlahBug;

            return dev;
        }
    }

    /// <summary>
    /// DeveloperFullTime - INHERITANCE dari abstract class Developer
    /// Implements specific calculation for Full Time developers
    /// </summary>
    public class DeveloperFullTime : Developer
    {
        public DeveloperFullTime() : base()
        {
            _statusKontrak = "Full Time";
        }

        /// <summary>
        /// POLYMORPHISM - implementasi HitungSkor untuk Full Time
        /// Rumus: Skor = 10 × Fitur - 5 × Bug
        /// Skor minimum adalah 0
        /// </summary>
        public override double HitungSkor()
        {
            double skor = (10.0 * _fiturSelesai) - (5.0 * _jumlahBug);
            return Math.Max(0, skor); // Skor tidak boleh negatif
        }

        /// <summary>
        /// POLYMORPHISM - implementasi HitungGaji untuk Full Time
        /// Rumus: Total Gaji = Gaji Pokok + Skor × Rp20.000,00
        /// Gaji Pokok = Rp5.000.000,00
        /// </summary>
        public override decimal HitungGaji()
        {
            double skor = HitungSkor();
            return GAJI_POKOK_FULLTIME + (decimal)(skor * 20000.0);
        }
    }

    /// <summary>
    /// DeveloperFreelance - INHERITANCE dari abstract class Developer
    /// Implements specific calculation for Freelance developers
    /// </summary>
    public class DeveloperFreelance : Developer
    {
        public DeveloperFreelance() : base()
        {
            _statusKontrak = "Freelance";
        }

        /// <summary>
        /// POLYMORPHISM - implementasi HitungSkor untuk Freelance
        /// Rumus: Skor = 100 × (1 - (2 × Bug) / (3 × Fitur))
        /// Skor minimum adalah 0, bisa lebih dari 100
        /// </summary>
        public override double HitungSkor()
        {
            // Jika fitur = 0, skor = 0
            if (_fiturSelesai == 0)
                return 0;

            double skor = 100.0 * (1.0 - (2.0 * _jumlahBug) / (3.0 * _fiturSelesai));
            return Math.Max(0, skor); // Skor tidak boleh negatif
        }

        /// <summary>
        /// POLYMORPHISM - implementasi HitungGaji untuk Freelance
        /// Rumus berdasarkan skor:
        /// - Skor >= 80: Rp500.000,00 × Fitur
        /// - 50 <= Skor < 80: Rp400.000,00 × Fitur
        /// - Skor < 50: Rp250.000,00 × Fitur
        /// </summary>
        public override decimal HitungGaji()
        {
            double skor = HitungSkor();
            decimal ratePerFitur;

            if (skor >= 80)
                ratePerFitur = 500000.00m;
            else if (skor >= 50)
                ratePerFitur = 400000.00m;
            else
                ratePerFitur = 250000.00m;

            return ratePerFitur * _fiturSelesai;
        }
    }
}
