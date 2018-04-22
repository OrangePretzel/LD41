using InControl;
using System.Collections;
using UnityEngine;

namespace LD41
{
	public class InputHelper : MonoBehaviour
	{
		private static bool WasSelectDown = false;
		private static bool WasMenuDown = false;
		private static bool WasBackDown = false;

		public const string KEYBOARD_AND_MOUSE = "Keyboard and Mouse";

		private class InputMode
		{
			public bool IsKeyboardAndMouse;
			public InControl.InputDevice InputDevice;
		}

		#region Singleton

		private static InputHelper instance;

		private void MakeSingleton()
		{
			if (instance != null && instance != this)
			{
				Destroy(this);
			}

			instance = this;
		}

		#endregion

		private const int MAX_PLAYERS = 4;

		public static System.Action<int> OnResetPlayerDevices;

		private InputMode[] playerDevices = new InputMode[MAX_PLAYERS];
		private PlayerInput[] playerInputs = new PlayerInput[MAX_PLAYERS];

		#region Unity Hooks

		private void Awake()
		{
			MakeSingleton();

			ResetPlayerDevices();
			InControl.InputManager.OnDeviceAttached += inputDevice =>
			{
				//Debug.Log($"Device [{inputDevice.Name}] attached");
			};
			InControl.InputManager.OnDeviceDetached += inputDevice =>
			{
				//Debug.Log($"Device [{inputDevice.Name}] detached");
				bool needsReset = false;
				foreach (var playerDevice in playerDevices)
				{
					if (playerDevice?.InputDevice == inputDevice)
					{
						needsReset = true;
						break;
					}
				}

				if (needsReset)
				{
					ResetPlayerDevices();
				}
				Debug.Log($"All devices reset!");
			};

			StartCoroutine(UpdateLoop());
		}

		private void OnGUI()
		{
			const bool DEBUG_GUI = true;
			if (DEBUG_GUI)
			{
				GUI.Label(new Rect(0, 0, 1000, 1000), $@"
Player 1:
{GetPlayerInput(0)}

Player 2:
{GetPlayerInput(1)}

Player 3:
{GetPlayerInput(2)}

Player 4:
{GetPlayerInput(3)}
");
			}
		}

		private IEnumerator UpdateLoop()
		{
			while (true)
			{
				ListenForPlayers();
				UpdatePlayerInputs();
				yield return new WaitForEndOfFrame();
			}
		}

		#endregion

		public static PlayerInput GetPlayerInput(int playerID)
		{
			return instance.playerInputs[playerID];
		}

		public static void DisconnectPlayerDevice(int playerID)
		{
			instance.playerDevices[playerID] = null;
			instance.playerInputs[playerID] = new PlayerInput() { PlayerID = playerID };
		}

		private void ResetPlayerDevices()
		{
			int numberOfOldDevices = 0;
			for (int i = 0; i < MAX_PLAYERS; i++)
			{
				if (playerDevices[i] != null)
					numberOfOldDevices++;

				playerDevices[i] = null;
				playerInputs[i] = new PlayerInput() { PlayerID = i };
			}

			OnResetPlayerDevices?.Invoke(numberOfOldDevices);
		}

		private void SetNextKeyboardPlayer()
		{
			const int INVALID = -1;
			int potentialPlayerID = INVALID;

			for (int i = 0; i < MAX_PLAYERS; i++)
			{
				if (playerDevices[i]?.IsKeyboardAndMouse ?? false)
					return;
				else if (playerDevices[i] == null && potentialPlayerID == INVALID)
					potentialPlayerID = i;
			}

			if (potentialPlayerID != INVALID)
			{
				//Debug.Log($"Player {potentialPlayerID + 1} controller set to Keyboard and Mouse");
				playerDevices[potentialPlayerID] = new InputMode()
				{
					IsKeyboardAndMouse = true,
					InputDevice = null
				};
			}
		}

		private void SetNextControllerPlayer(InputDevice inputDevice)
		{
			const int INVALID = -1;
			int potentialPlayerID = INVALID;

			for (int i = 0; i < MAX_PLAYERS; i++)
			{
				if (playerDevices[i]?.InputDevice == inputDevice)
					return;
				else if (playerDevices[i] == null && potentialPlayerID == INVALID)
					potentialPlayerID = i;
			}

			if (potentialPlayerID != INVALID)
			{
				//Debug.Log($"Player {potentialPlayerID + 1} controller set to {inputDevice.Name}");
				playerDevices[potentialPlayerID] = new InputMode()
				{
					IsKeyboardAndMouse = false,
					InputDevice = inputDevice
				};
			}
		}

		public void ListenForPlayers()
		{
			foreach (var inputDevice in InControl.InputManager.Devices)
			{
				if (!inputDevice.IsAttached || !inputDevice.IsKnown || !inputDevice.IsSupportedOnThisPlatform)
					continue;

				if (inputDevice.Command)
				{
					SetNextControllerPlayer(inputDevice);
				}
			}

			if (Input.GetKey(KeyCode.Space))
				SetNextKeyboardPlayer();
		}

		private void UpdatePlayerInputs()
		{
			for (int i = 0; i < MAX_PLAYERS; i++)
			{
				var inputMode = playerDevices[i];
				if (inputMode == null)
					continue;

				if (inputMode.IsKeyboardAndMouse)
					UpdateKeyboardAndMousePlayerInputs(i);
				else
					UpdateControllerPlayerInputs(i, inputMode.InputDevice);
			}
		}

		private void UpdateKeyboardAndMousePlayerInputs(int playerID)
		{
			var playerInput = playerInputs[playerID];
			playerInput.PlayerDevice = KEYBOARD_AND_MOUSE;

			playerInput.HorizontalMovement =
				Input.GetKey(KeyCode.A) ? -1 : 0 +
				(Input.GetKey(KeyCode.D) ? 1 : 0);
			playerInput.VerticalMovement =
				Input.GetKey(KeyCode.S) ? -1 : 0 +
				(Input.GetKey(KeyCode.W) ? 1 : 0);

			playerInput.HorizontalAim = Input.mousePosition.x;
			playerInput.VerticalAim = Input.mousePosition.y;

			playerInput.Jump = Input.GetKey(KeyCode.W);
			playerInput.Shooting = Input.GetKey(KeyCode.Mouse0);

			var menuDown = Input.GetKey(KeyCode.Escape);
			playerInput.Menu = !WasMenuDown && menuDown;
			WasMenuDown = menuDown;

			var selectDown = Input.GetKey(KeyCode.Space);
			playerInput.Select = !WasSelectDown && selectDown;
			WasSelectDown = selectDown;

			var backDown = Input.GetKey(KeyCode.Backspace);
			playerInput.Back = !WasBackDown && backDown;
			WasBackDown = backDown;
		}

		private void UpdateControllerPlayerInputs(int playerID, InControl.InputDevice inputDevice)
		{
			if (inputDevice == null)
				return;

			var playerInput = playerInputs[playerID];
			playerInput.PlayerDevice = inputDevice.Name;

			playerInput.HorizontalMovement = inputDevice.LeftStick.X;
			playerInput.VerticalMovement = inputDevice.LeftStick.Y;

			playerInput.HorizontalAim = inputDevice.RightStick.X;
			playerInput.VerticalAim = inputDevice.RightStick.Y;

			playerInput.Jump = inputDevice.LeftTrigger.IsPressed;
			playerInput.Shooting = inputDevice.RightTrigger.IsPressed;

			playerInput.Menu = inputDevice.Command.WasPressed;
			playerInput.Select = inputDevice.Action1.WasPressed;
			playerInput.Back = inputDevice.Action2.WasPressed;
		}
	}

	public class PlayerInput
	{
		public string PlayerDevice;
		public int PlayerID;

		public float HorizontalMovement;
		public float VerticalMovement;
		public float HorizontalAim;
		public float VerticalAim;

		public bool Jump;
		public bool Shooting;

		public bool Menu;
		public bool Select;
		public bool Back;

		public Vector3 GetNormalizedAim(Vector3 originPoint, Camera screenCamera)
		{
			if (PlayerDevice != InputHelper.KEYBOARD_AND_MOUSE)
				return new Vector3(HorizontalAim, VerticalAim).normalized;

			var aimVec = (screenCamera.ScreenToWorldPoint(new Vector3(HorizontalAim, VerticalAim)) - originPoint);
			aimVec.z = 0;
			aimVec = aimVec.normalized;
			Debug.Log($"Normalized Aim Vector = {aimVec}");
			return aimVec;
		}

		public override string ToString()
		{
			return $@"Player: ({PlayerID})
Input Device: ({PlayerDevice})
Movement: ({HorizontalMovement}, {VerticalMovement})
Aim: ({HorizontalAim}, {VerticalAim})
Jump: ({Jump})
Shooting: ({Shooting})
Menu: ({Menu})
Select: ({Select})
Back: ({Back})";
		}
	}
}
