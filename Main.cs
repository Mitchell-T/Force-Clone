using MelonLoader;
using UnityEngine;
using VRC.Core;
using VRC;
using UnityEngine.UI;
using VRC.UI;
using VRC.Core.BestHTTP;

namespace ForceClone
{
    internal class fc : MelonMod
    {
        private Transform AddMenuButton(string butName, Transform parent, string butText, string tooltip, int butX, int butY, System.Action butAction)
        {
            Transform quickMenu = QuickMenu.prop_QuickMenu_0.transform;

            // clone of a standard button
            Transform butTransform = UnityEngine.Object.Instantiate(quickMenu.Find("CameraMenu/BackButton").gameObject).transform;

            // Set internal name of button
            butTransform.name = butName;

            // set button's parent to quick menu
            butTransform.SetParent(parent, false);

            // set button's text
            butTransform.GetComponentInChildren<Text>().text = butText;
            butTransform.GetComponent<UiTooltip>().text = tooltip;
            butTransform.GetComponent<UiTooltip>().alternateText = tooltip;

            // set position of new button based on existing menu buttons
            float buttonWidth = quickMenu.Find("UserInteractMenu/ForceLogoutButton").localPosition.x - quickMenu.Find("UserInteractMenu/BanButton").localPosition.x;
            float buttonHeight = quickMenu.Find("UserInteractMenu/ForceLogoutButton").localPosition.x - quickMenu.Find("UserInteractMenu/BanButton").localPosition.x;
            butTransform.localPosition = new Vector3(butTransform.localPosition.x + buttonWidth * butX, butTransform.localPosition.y + buttonHeight * butY, butTransform.localPosition.z);

            // Make it so the button does what we want
            butTransform.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            butTransform.GetComponent<Button>().onClick.AddListener(butAction);

            // enable it just in case
            butTransform.gameObject.SetActive(true);

            return butTransform;
        }

        public static Player GetPlayer(string UserID)
        {
            var Players = PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0;
            Player FoundPlayer = null;
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                if (player.field_Private_APIUser_0.id == UserID)
                {
                    FoundPlayer = player;
                }
            }

            return FoundPlayer;
        }

        public static void showDialog(string title, string message)
        {
            Resources.FindObjectsOfTypeAll<VRCUiPopupManager>()[0].Method_Public_Void_String_String_Single_3(title, message, 10f);
        }

        public override void OnApplicationStart()
        {
            MelonModLogger.Log("Force Clone mod started");
        }

        public override void VRChat_OnUiManagerInit()
        {
            this.AddMenuButton("fcQuickMenu", QuickMenu.prop_QuickMenu_0.transform.Find("UserInteractMenu"), "<color=white>Force Clone Avatar</color>", "Forces the cloning of a public avatar", 0, 1, new System.Action(() =>
            {
                var SelectedPlayer = GetPlayer(QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0.id);
                string AvatarID = SelectedPlayer.prop_VRCAvatarManager_0.field_Private_ApiAvatar_0.id;

                if (SelectedPlayer.prop_VRCAvatarManager_0.field_Private_ApiAvatar_0.releaseStatus != "private")
                {
                    MelonModLogger.Log("Force Cloning avatar with ID: " + AvatarID);
                    VRC.Core.API.SendRequest($"avatars/{AvatarID}", HTTPMethods.Get, new ApiModelContainer<ApiAvatar>(), null, true, true, 3600f, 2, null);
                    new PageAvatar
                    {
                        avatar = new SimpleAvatarPedestal
                        {
                            field_Internal_ApiAvatar_0 = new ApiAvatar
                            {
                                id = AvatarID
                            }
                        }
                    }.ChangeToSelectedAvatar();
                    VRCUiManager.prop_VRCUiManager_0.Method_Public_Void_Boolean_2(false);
                }
                else
                {
                    MelonModLogger.Log("Avatar ID " + AvatarID + "is private :(");
                    showDialog("<color=red>Error!</color>", "<color=white>Avatar ID " + AvatarID + " is private!</color>");
                }
            }));
        }
    }
}