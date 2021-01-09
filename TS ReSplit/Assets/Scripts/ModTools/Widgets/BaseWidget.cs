namespace ModTools.Widgets
{
    public class BaseWidget
    {
        public static int WidgetUuidNext;
        
        public FileEntry FileEntryForWidget;
        public bool      ShouldClose = false;
        public int       WidgetUuid  = WidgetUuidNext++;
        public virtual string    WidgetName => "";

        public BaseWidget(FileEntry fileEntryForWidget = null)
        {
            FileEntryForWidget = fileEntryForWidget;
        }

        public virtual void Draw()
        {
        }
    }
}