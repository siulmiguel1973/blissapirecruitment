//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BlissApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Choices
    {

        public int choiceID { get; set; }
        public string choice { get; set; }
        public int votes { get; set; }
        public int questionsID { get; set; }
    
        public virtual questions questions { get; set; }
    }
}