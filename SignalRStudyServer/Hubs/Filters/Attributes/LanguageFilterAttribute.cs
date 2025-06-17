namespace SignalRStudyServer.Hubs.Filters.Attributes;

// 특정 메서드에만 필터를 적용하기 위한 어트리뷰트
[AttributeUsage(AttributeTargets.Method)]
public class LanguageFilterAttribute : Attribute
{
    public int FilterArgument { get; set; }
    
    public LanguageFilterAttribute(int filterArgument)
    {
        FilterArgument = filterArgument;
    }
}