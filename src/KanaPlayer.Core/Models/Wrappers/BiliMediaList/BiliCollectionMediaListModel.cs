using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BiliCollectionMediaListModel : CommonApiModel<BiliCollectionMediaListDataModel>;

public class BiliCollectionMediaListDataModel
{
    [JsonPropertyName("info")] public required BiliMediaListCommonInfoModel Info { get; set; }
    
    // 存在分页设计，请在 API 中指定是否需要获取完整
    [JsonPropertyName("medias")] public required List<BiliMediaListCommonMediaModel> Medias { get; set; }
}