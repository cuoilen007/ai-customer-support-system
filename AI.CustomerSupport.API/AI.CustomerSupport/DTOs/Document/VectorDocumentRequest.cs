namespace AI.CustomerSupport.API.DTOs.Document
{
    public class VectorDocumentRequest
    {
        public string DocumentId { get; set; }
       = string.Empty;

        public string Content { get; set; }
            = string.Empty;
    }
}
