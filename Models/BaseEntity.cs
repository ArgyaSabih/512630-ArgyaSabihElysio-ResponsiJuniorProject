using System;

namespace ResponsiJuniorProject.Models
{
    /// <summary>
    /// Abstract base class for all entities - demonstrates INHERITANCE in OOP
    /// All entity classes (Proyek, Developer) inherit from this class
    /// </summary>
    public abstract class BaseEntity
    {
        // Protected field - accessible by derived classes (inheritance)
        protected int _id;

        // Protected property for ID - encapsulation
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        // Property to track creation timestamp
        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        // Default constructor
        protected BaseEntity()
        {
            _createdAt = DateTime.Now;
        }

        /// <summary>
        /// Abstract method - must be implemented by derived classes (inheritance + polymorphism)
        /// </summary>
        public abstract string GetInfo();

        /// <summary>
        /// Virtual method - can be overridden by derived classes
        /// </summary>
        public virtual bool IsValid()
        {
            return _id >= 0;
        }
    }
}
