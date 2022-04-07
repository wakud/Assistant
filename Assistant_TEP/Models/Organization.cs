using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assistant_TEP.Models
{
    /// <summary>
    /// Таблиця організацій
    /// </summary>
    [Table("Organization")]
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }        //назва організації

        [StringLength(50)]
        public string NmeDoc { get; set; }      //назва організації для документів в родовому відмінку

        public int RegionId { get; set; }       //код району

        [StringLength(4)]
        public string Code { get; set; }        //код організації

        [StringLength(50)]
        public string Nach { get; set; }        //ПІП начальника

        [StringLength(100)]
        public string Address { get; set; }     //адреса організації

        [StringLength(50)]
        public string Buh { get; set; }         //ПІП бухгалтера

        [StringLength(10)]
        public string Tel { get; set; }         //телефон організації   
        public string Postal { get; set; }      //індекс
        public string Rah_Iban { get; set; }    //р/р організації
        public List<User> Users { get; set; }   //список користувачів організації
        /// <summary>
        /// користувачі організації
        /// </summary>
        public Organization()
        {
            Users = new List<User>();
        }
    }
}
