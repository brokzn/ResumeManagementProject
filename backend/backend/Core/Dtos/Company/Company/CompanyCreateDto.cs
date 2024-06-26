using backend.Core.Enums;

namespace backend.Core.Dtos.Company.Company
{
    public class CompanyCreateDto
    {
        public string Name { get; set; }
        public CompanySize Size { get; set; }
    }
}
