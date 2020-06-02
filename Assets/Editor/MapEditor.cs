using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var mapGenerator = (MapGenerator)target;
        mapGenerator.GenerateMap();
    }
}