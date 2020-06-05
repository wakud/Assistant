using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assistant_TEP.Models
{
    [Table("Organization")]
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(4)]
        public string Code { get; set; }

        [StringLength(50)]
        public string Nach { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Buh { get; set; }

        [StringLength(10)]
        public string Tel { get; set; }

        public List<User> Users { get; set; }

        public Organization()
        {
            Users = new List<User>();
        }
    }
}
