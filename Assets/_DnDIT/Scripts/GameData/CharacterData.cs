using System.Collections.Generic;

namespace DnDInitiativeTracker.GameData
{
    public record CharacterData(string ImagePath, string Name, List<string> AudioPathList);
}