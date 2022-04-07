using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// список абонентів для укрпошти (форма 103)
    /// </summary>
    [Table("Abonents")]
    public class Abonents
    {
        [Key]
        public int Id { get; set; }

        public string CodeCok { get; set; }                 // код цоку

        public string OsRah { get; set; }                   //особовий рахунок абонента

        public string FullName { get; set; }                //ПІП абонента

        public string LastName { get; set; }                // прізвище

        public string FirstName { get; set; }               // ім'я

        public string SecondName { get; set; }              // по батькові

        public string FullAddress { get; set; }             //повна адреса абонента

        public int PostalCode { get; set; }                //індекс

        public string Oblast { get; set; }                  // область

        public string Rajon { get; set; }                   //район

        public string TypeOfCityFull { get; set; }          // тип населеного пункту повністю (село або місто і т.д.)

        public string TypeOfCityAbbr { get; set; }          //тип населеного пункту скорочено

        public string City { get; set; }                    // назва населеного пункту

        public string TypVul { get; set; }                  // тип вулиці скорочено

        public string TypeStreet { get; set; }              // тип вулиці повністю

        public string Street { get; set; }                  // вулиця

        public string House { get; set; }                   //будинок

        public string Housing { get; set; }                 // корпус

        public string Apartment { get; set; }               //квартира

        public bool Juridical { get; set; }                 //чи юридичний абонент

        public string SumaStr { get; set; }                 //сума за доставку

        public int UserId { get; set; }                     // айді користувача хто створив
    }
}
