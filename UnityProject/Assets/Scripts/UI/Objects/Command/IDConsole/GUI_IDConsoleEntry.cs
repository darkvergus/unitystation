using System.Linq;
using Systems.Clearance;
using UnityEngine;
using UnityEngine.UI;
using UI.Core.NetUI;

namespace UI.Objects.Command
{
	/// <summary>
	/// New ID console entry. Manages the logic of an individual button on the ID
	/// console, which may be an assignment (like HOS, Miner, etc...) or an individual access (mining office, rnd lab, etc...)
	/// </summary>
	public class GUI_IDConsoleEntry : MonoBehaviour
	{
		//This button is used in two types - as access and assignment
		[Tooltip("Whether this is an assignment (occupation) or an access (individual permission)")]
		[SerializeField]
		private bool isOccupation = false;
		[Tooltip("If assignment, occupation this button will grant.")]
		[SerializeField]
		private Occupation occupation = null;

		[Tooltip("If clearance, clearance this button will grant")]
        [SerializeField]
        private Clearance clearance = Clearance.MaintTunnels;

		[Tooltip("Color settings to apply when it's on")]
		[SerializeField]
		[Header("On Colors")]
		private ColorBlock onColors = ColorBlock.defaultColorBlock;

		[Tooltip("Color settings to use when it's off")]
		[SerializeField]
		[Header("Off Colors")]
		private ColorBlock offColors = ColorBlock.defaultColorBlock;

		//parent ID console tab this lives in
		private GUI_IDConsole console;
		private IDCard TargetCard => console.TargetCard;
		/// <summary>
		/// True if this entry is for an individual Access
		/// </summary>
		public bool IsAccess => !isOccupation;
		/// <summary>
		/// True if this entry is for an entire occupation
		/// </summary>
		public bool IsOccupation => isOccupation;

		/// <summary>
		/// If IsAccess, access this entry controls
		/// </summary>
		public Clearance Clearance => clearance;
		/// <summary>
		/// If IsOccupation, occupation this entry controls
		/// </summary>
		public Occupation Occupation => occupation;


		private Toggle toggle;
		private NetToggle netToggle;

		private void Awake()
		{
			console = GetComponentInParent<GUI_IDConsole>();
			toggle = GetComponentInChildren<Toggle>();
			netToggle = GetComponentInChildren<NetToggle>();
			//annoyingly, the built in Toggle has no way to just change color when it is selected, so we have
			//to add custom logic to do this
			toggle.onValueChanged.AddListener(OnToggleValueChanged);
			OnToggleValueChanged(toggle.isOn);
		}

		private void OnToggleValueChanged(bool isOn)
		{
			toggle.colors = isOn ? onColors : offColors;
			//occupations which are on are not clickable
			if (IsOccupation)
			{
				toggle.interactable = !isOn;
			}

		}

		public void ServerToggle(bool isToggled)
		{
			if (isOccupation)
			{
				if (isToggled)
				{
					console.ServerChangeAssignment(occupation);
				}
			}
			else if (!isOccupation)
			{
				console.ServerModifyAccess(clearance, isToggled);
			}
		}

		/// <summary>
		/// Refreshes the status of this entry based on the access / occupation of the target card
		/// </summary>
		public void ServerRefreshFromTargetCard()
		{
			//we check for current toggle status just to make sure we don't pointlessly send
			//a message when the value hasn't changed

			//no card inserted, nothing should be on
			if (TargetCard == null)
			{
				if (toggle.isOn)
				{
					netToggle.MasterSetValue("0");
				}
				return;
			}

			if (isOccupation)
			{
				var hasOccupation = TargetCard.Occupation == occupation;
				if (hasOccupation && !toggle.isOn)
				{
					netToggle.MasterSetValue("1");
				}
				else if (!hasOccupation && toggle.isOn)
				{
					netToggle.MasterSetValue("0");
				}
			}
			else
			{
				var source = (IClearanceSource)TargetCard.ClearanceSource;
				var containsClearance = source.GetCurrentClearance.Contains(clearance);

				if (containsClearance && !toggle.isOn)
				{
					netToggle.MasterSetValue("1");
				}
				else if (containsClearance == false && toggle.isOn)
				{
					netToggle.MasterSetValue("0");
				}
			}
		}
	}
}
