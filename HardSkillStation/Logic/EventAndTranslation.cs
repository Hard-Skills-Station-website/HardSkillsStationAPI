using HardSkillStation.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace HardSkillStation.Logic
{
    public class EventAndTranslation
    {
        public Event Event { get; set; }
        public EventTranslation Translation { get; set; }
    }
    public class EventCreationModel
    {
        // Properties for the Event
        public string? EventImage { get; set; }
        public int? EventCategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public string? EventLocation { get; set; }
        public string? EventCompany { get; set; }
        public string? EventContact { get; set; }
        public bool EventIsPublished { get; set; }

        // Properties for the EventTranslation
        public string? TranslationHeadlineDanish { get; set; }
        public string? TranslationSummaryDanish { get; set; }
        public string? TranslationDescriptionDanish { get; set; }
        public string? TranslationHeadlineEnglish { get; set; }
        public string? TranslationSummaryEnglish { get; set; }
        public string? TranslationDescriptionEnglish { get; set; }
    }
    public class EventTranslationModel
    {
        public string Language { get; set; }
        public string Headline { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
    }

}
