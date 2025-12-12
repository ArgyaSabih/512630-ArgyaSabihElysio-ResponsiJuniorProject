using System;

namespace ResponsiJuniorProject.Models
{
    /// <summary>
    /// Proyek (Project) class - inherits from BaseEntity (inheritance OOP)
    /// Contains encapsulated properties (encapsulation OOP)
    /// </summary>
    public class Proyek : BaseEntity
    {
        // Private fields - encapsulation
        private string _namaProyek;
        private string _client;
        private decimal _budget;

        // Properties with encapsulation
        public int IdProyek
        {
            get { return _id; }
            set { _id = value; }
        }

        public string NamaProyek
        {
            get { return _namaProyek; }
            set { _namaProyek = value; }
        }

        public string Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public decimal Budget
        {
            get { return _budget; }
            set
            {
                if (value >= 0)
                    _budget = value;
                else
                    throw new ArgumentException("Budget tidak boleh negatif");
            }
        }

        // Default constructor
        public Proyek()
        {
        }

        // Parameterized constructor
        public Proyek(int idProyek, string namaProyek, string client, decimal budget)
        {
            IdProyek = idProyek;
            NamaProyek = namaProyek;
            Client = client;
            Budget = budget;
        }

        // Override abstract method from base class
        public override string GetInfo()
        {
            return $"Proyek: {NamaProyek}, Client: {Client}, Budget: Rp {Budget:N0}";
        }

        // Override ToString for ComboBox display
        public override string ToString()
        {
            return NamaProyek;
        }
    }
}
