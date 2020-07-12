namespace Gardiner.VsShowMissing
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidGardiner_VsShowMissingPkgString = "36b8e40e-ceaa-4305-bdb7-1a68a0d84067";
        public static Guid guidGardiner_VsShowMissingPkg = new Guid(guidGardiner_VsShowMissingPkgString);

        public const string guidVsShowMissingPackageCmdSetString = "5faea57f-7fa2-4ac2-99b8-e93b226d14cb";
        public static Guid guidVsShowMissingPackageCmdSet = new Guid(guidVsShowMissingPackageCmdSetString);

        public const string guidGardiner_ErrorListCmdSetString = "a4d9c40f-e108-425c-8c72-9f99c53e3643";
        public static Guid guidGardiner_ErrorListCmdSet = new Guid(guidGardiner_ErrorListCmdSetString);

        public const string guidImagesString = "06666f0f-f64c-4155-bf46-9fe077a90109";
        public static Guid guidImages = new Guid(guidImagesString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int MyMenuGroup = 0x1020;
        public const int cmdidFindMissingFiles = 0x0100;
        public const int MyErrorListMenuGroup = 0x1021;
        public const int cmdidDeleteFileOnDisk = 0x0200;
        public const int cmdidIncludeFileInProject = 0x0201;
        public const int cmdidExcludeFileFromProject = 0x0202;
        public const int bmpPic1 = 0x0001;
        public const int bmpPic2 = 0x0002;
        public const int bmpPicSearch = 0x0003;
        public const int bmpPicX = 0x0004;
        public const int bmpPicArrows = 0x0005;
        public const int bmpPicStrikethrough = 0x0006;
    }
}