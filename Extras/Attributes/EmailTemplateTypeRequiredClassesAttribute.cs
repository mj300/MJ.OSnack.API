using System;

namespace OSnack.API.Extras.Attributes
{
   [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
   public class EmailTemplateTypeRequiredClassesAttribute : Attribute
   {

      public Type[] ClassTypes { get; set; }

      public EmailTemplateTypeRequiredClassesAttribute(params Type[] classTypes)
      {
         ClassTypes = classTypes;
      }
   }
}
