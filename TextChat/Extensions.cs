using Smod2.API;
using UnityEngine;

namespace TextChat
{
	public static class Extensions
	{
		public static GameObject GameObject(this Player player) => (GameObject) player.GetGameObject();
	}
}