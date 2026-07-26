using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Raccoin.UI.Title
{
    /// <summary>
    /// 标题画面控制器 - 处理标题界面的按钮交互
    /// 由 SceneBuilder 在构建标题场景时自动挂载并连线
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [Header("按钮引用 (由 SceneBuilder 赋值)")]
        public Button PlayButton;
        public Button LabButton;
        public Button OptionsButton;

        [Header("版本信息")]
        public Text VersionText;

        private void Start()
        {
            if (PlayButton != null) PlayButton.onClick.AddListener(OnPlayClicked);
            if (LabButton != null) LabButton.onClick.AddListener(OnLabClicked);
            if (OptionsButton != null) OptionsButton.onClick.AddListener(OnOptionsClicked);

            if (VersionText != null)
                VersionText.text = $"v{RabbitBuildVersion.VERSION} ({RabbitBuildVersion.BUILD})";

            Debug.Log("[TitleScreen] Title screen ready.");
        }

        /// <summary>进入经典推币机模式 (场景索引 2 = GameClassic)</summary>
        public void OnPlayClicked()
        {
            Debug.Log("[TitleScreen] Play clicked -> GameClassic");
            SceneManager.LoadScene(2);
        }

        /// <summary>进入实验室模式 (场景索引 3 = GameLab)</summary>
        public void OnLabClicked()
        {
            Debug.Log("[TitleScreen] Lab clicked -> GameLab");
            SceneManager.LoadScene(3);
        }

        public void OnOptionsClicked()
        {
            Debug.Log("[TitleScreen] Options clicked (not implemented yet)");
        }
    }
}
