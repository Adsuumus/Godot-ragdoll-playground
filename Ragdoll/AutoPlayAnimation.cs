using Godot;

public partial class AutoPlayAnimation : AnimationPlayer
{
	public override void _Ready()
	{
		 if (Engine.IsEditorHint()) return;
		Play("Clip1");
	}
}
