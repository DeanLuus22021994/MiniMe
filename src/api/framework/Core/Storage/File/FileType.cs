using System.ComponentModel;

namespace FSH.Framework.Core.Storage.File;

public enum FileType
{
    [Description(".jpg,.png,.jpeg")]
    Image,
    [Description(".pdf,.doc,.docx,.txt,.xlsx,.xls,.csv")]
    Document,
    [Description(".mp4,.avi,.mov,.wmv,.flv,.mkv")]
    Video,
    [Description(".mp3,.wav,.flac,.aac,.ogg")]
    Audio,
    [Description(".*")]
    Other
}
