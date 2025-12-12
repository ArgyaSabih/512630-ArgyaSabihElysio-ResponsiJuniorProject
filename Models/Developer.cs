using System;

namespace ResponsiJuniorProject.Models
{
    /// <summary>
    /// Developer class - inherits from BaseEntity (inheritance OOP)
    /// Contains encapsulated properties (encapsulation OOP)
    /// </summary>
    public class Developer : BaseEntity
    {
        // Private fields - encapsulation
        private int _idProyek;
        private string _namaDev;
        private string _statusKontrak;
        private int _fiturSelesai;
        private int _jumlahBug;

        // Properties with encapsulation
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
            set
            {
                // Validate status kontrak
                if (value == "Freelance" || value == "Full Time")
                    _statusKontrak = value;
                else
                    throw new ArgumentException("Status kontrak harus 'Freelance' atau 'Full Time'");
            }
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

        // Default constructor
        public Developer()
        {
        }

        // Parameterized constructor
        public Developer(int idDev, int idProyek, string namaDev, string statusKontrak, int fiturSelesai, int jumlahBug)
        {
            IdDev = idDev;
            IdProyek = idProyek;
            NamaDev = namaDev;
            StatusKontrak = statusKontrak;
            FiturSelesai = fiturSelesai;
            JumlahBug = jumlahBug;
        }

        // Override abstract method from base class
        public override string GetInfo()
        {
            return $"Developer: {NamaDev}, Status: {StatusKontrak}, Fitur: {FiturSelesai}, Bug: {JumlahBug}";
        }

        // Override ToString
        public override string ToString()
        {
            return NamaDev;
        }
    }
}
