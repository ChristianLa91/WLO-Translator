namespace WLO_Translator_WPF
{
    public static class Constants
    {
        // ActionInfo.dat
        public const int    ACTIONINFO_DATA_LENGTH_PER_ITEM                         = 294;
        public const int    ACTIONINFO_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH  = 18;//29;
        public const int    ACTIONINFO_DATA_ID_OFFSET                               = 34; // From item start
        public const int    ACTIONINFO_DATA_INITIAL_OFFSET                          = 1;

        // aLogin.exe
        public const int    ALOGIN_EXE_LENGTH_PER_ITEM                              = 100;
        public const int    ALOGIN_EXE_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH       = -1;//29;

        // Item.dat
        public const int    ITEM_DATA_LENGTH_PER_ITEM                               = 451;
        public const int    ITEM_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH        = 131;

        // Mark.dat
        public const int    MARK_DATA_LENGTH_PER_ITEM                               = 553;
        public const int    MARK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH        = 6;

        // Npc.dat
        public const int    NPC_DATA_LENGTH_PER_ITEM                                = 138;
        public const int    NPC_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH         = -1;

        // SceneData.dat
        public const int    SCENEDATA_DATA_LENGTH_PER_ITEM                          = 131;
        public const int    SCENEDATA_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH   = -1;
        public const int    SCENEDATA_DATA_ID_OFFSET                                = 107;
        public const int    SCENEDATA_DATA_INITIAL_OFFSET                           = 2;

        // Skill.dat
        public const int    SKILL_DATA_LENGTH_PER_ITEM                              = 148;
        public const int    SKILL_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH       = 45;

        // Talk.dat
        public const int    TALK_DATA_LENGTH_PER_ITEM                               = 292;
        public const int    TALK_DATA_AFTER_NAME_TO_DESCRIPTIONLENGTH_LENGTH        = -1;
        public const int    TALK_DATA_ID_OFFSET                                     = 34;
        public const int    TALK_DATA_INITIAL_OFFSET                                = 2;

        public const int    LOADING_SLEEP_LENGTH                                    = 80;

        public const char   CHAR_TO_REPLACE_NULL_CHAR                               = '.';
        public const char   CHAR_TO_REPLACE_OTHER_CHAR                              = '*';

        //File Endings
        public const string FILE_ENDING_STORED_ITEM_DATA                            = ".wtsi"; // WLO Translate Saved Items
    }
}
