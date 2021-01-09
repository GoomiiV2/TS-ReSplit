namespace ModTools.Widgets
{
    public class ModelViewer : ThreeDViewport
    {
        public override string WidgetName =>
            $"Model Editor: {FileEntryForWidget.Name}##{FileEntryForWidget.FullPath}-{WidgetUuid}";

        [FileActionAttribute("OpenPs2Ts2Model", Platforms.TS2_PS2, FileTypes.Mesh, EntryTypes.File,
            FileActionAttribute.ActionTypes.DoubleClick)]
        public static bool OpenPs2Ts2Model(FileEntry fe)
        {
            var widget = new ModelViewer(fe);
            ModToolsMain.ModTools.OpenWidget(widget);
            
            return true;
        }

        public ModelViewer(FileEntry fe) : base(fe)
        {
            var model = ScenePrefabInst.GetComponent<AnimatedModelV2>();
            model.LoadModel();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}