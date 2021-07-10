using System;

namespace OSnack.API.Extras.Attributes
{
   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
   public class EmailTemplateVariableAttribute : Attribute
   {
      public string Name { get; set; }
      public string[] ListNames { get; set; }

   }
}
