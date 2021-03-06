﻿using System.Runtime.Serialization;

namespace Cats.Service.TemplateEditor.Dto
{
    [DataContract]
    public class TemplateType
    {
        
        [DataMember]
        public int TemplateTypeId { get; set; }
         [DataMember]
        public string TemplateObject { get; set; }
         [DataMember]
        public string Remark { get; set; }
    }
}
