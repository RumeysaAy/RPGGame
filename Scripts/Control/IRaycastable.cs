using System.Runtime.InteropServices;

namespace RPG.Control
{
    public interface IRaycastable
    {
        CursorType GetCursorType();
        bool HandleRaycast(PlayerController callingController);



    }

}