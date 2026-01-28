# SunnyLand - Unity 2D Platformer

## Architecture Overview

**Core Game Loop**: Player navigates platforms, collects gems, defeats enemies by jumping on them, and transitions between scenes. Gem count persists across scenes using singleton pattern.

**Component Structure**:
- [Assets/Scripts/PlayerController.cs](../Assets/Scripts/PlayerController.cs): State machine-driven player with physics-based movement
- [Assets/Scripts/Enemy.cs](../Assets/Scripts/Enemy.cs): Base class for enemies (Frog, Eagle) with shared death animation
- [Assets/PermanentUI.cs](../Assets/PermanentUI.cs): Singleton UI manager (`PermanentUI.perm`) for cross-scene state
- [Assets/Scripts/SceneChange.cs](../Assets/Scripts/SceneChange.cs): Handles scene transitions via trigger colliders or UI buttons
- [Assets/Scripts/CameraController.cs](../Assets/Scripts/CameraController.cs): Follows player Rigidbody2D position

## Critical Patterns

### State Management
- **Player states**: Uses private enum `{idle, running, jumping, falling, hurt}` with integer mapping to Animator
- **Enemy states**: Frog uses bool animator parameters (`Jumping`, `Falling`); Eagle is stateless
- **Animation sync**: Set animator parameters, then let AnimationState() handle transitions based on physics

### Physics & Collision
- **LayerMasks**: Always use `LayerMask.GetMask("Ground")` and `LayerMask.GetMask("EnemyLayer")` - critical for raycasts
- **Ground detection**: Use `Collider2D.IsTouchingLayers(ground)` or raycast down from collider center
- **Enemy defeat**: Raycast down 1.3f from player collider; if hit or state==falling, call `enemy.JumpedOn()`
- **Air control**: Player moves at `speed * airControl` (0.8x) when not grounded

### Singleton Pattern
```csharp
public static PermanentUI perm;
void Start() {
    DontDestroyOnLoad(gameObject);
    if(!perm) perm = this;
    else Destroy(gameObject);
}
```
Access globally: `PermanentUI.perm.gems++` or `PermanentUI.perm.Reset()`

### Enemy Inheritance
- Extend `Enemy` base class, call `base.Start()` to initialize Animator, Rigidbody2D, AudioSource
- Override `Start()` with `protected override void Start()` - not `new void Start()`
- **Frog**: Patrol with jump physics between `leftCap`/`rightCap`, called by Animation Event on `Move()`
- **Eagle**: Uses `InvokeRepeating("Shoot", 0f, shootCD)` to spawn projectiles toward player
- **EagleShot**: Coroutine `IEnumerator Start()` calculates velocity, auto-destroys after 5 seconds

## Developer Workflows

**Unity Version**: 2020.3.9f1 (critical for compatibility)

**Scene Setup**:
1. Add scenes to File > Build Settings before using SceneManager
2. Set tags: "Player", "Enemy", "Collectible"
3. Set layers: "Ground", "EnemyLayer"
4. Place prefabs from `Assets/Prefabs/` (Player, Frog, Eagle, Gem, Canvas)

**Audio Integration**: 
- Attach AudioSource components to GameObjects
- Serialize in inspector: `[SerializeField] private AudioSource jumpAudio`
- Trigger with `.Play()` - footstep audio triggered by Animation Event `Footstep()`

**Adding New Enemies**:
1. Inherit from Enemy: `public class NewEnemy : Enemy`
2. Call `base.Start()` in `protected override void Start()`
3. Implement movement/attack logic
4. Use `anim`, `rb`, `explosion` from base class

## Project-Specific Conventions

- **No null checks on PermanentUI.perm**: Code assumes it exists (placed in first scene)
- **Tag-based collision**: Check `collision.tag == "Collectible"` or `"Enemy"` - tags matter
- **Velocity manipulation**: Directly set `rb.velocity = new Vector2(x, y)` for movement/knockback
- **Scene reload on death**: Both Fall.cs and SceneChange.cs call `SceneManager.LoadScene(SceneManager.GetActiveScene().name)` and `PermanentUI.perm.Reset()`
- **Sprite flipping**: Use `transform.localScale = new Vector2(-1, 1)` for horizontal flip
- **Movement control**: Player can't move during hurt state (`if (state != State.hurt)`)

## Key Files & Structure

```
Assets/
  Scripts/
    PlayerController.cs    - 213 lines, state machine + physics
    Enemy.cs              - 28 lines, base class
    Frog.cs               - 86 lines, patrol jumper
    Eagle.cs              - 25 lines, shooting enemy
    EagleShot.cs          - 28 lines, homing projectile
    SceneChange.cs        - 44 lines, transitions + button handler
    Fall.cs               - 17 lines, death plane
    CameraController.cs   - 13 lines, simple follow
  PermanentUI.cs          - 26 lines, singleton UI manager
  Prefabs/                - Player, enemies, collectibles
  Scenes/                 - Game levels
```

## Common Tasks

**Modify player speed**: Change `speed = 9` and `airControl = 0.8f` in PlayerController
**Add collectible type**: Check tag in `PlayerController.OnTriggerEnter2D()`, update `PermanentUI`
**New scene transition**: Add SceneChange component to trigger collider, set `sceneName` in inspector
**Adjust enemy behavior**: Modify `leftCap`/`rightCap` (Frog) or `shootRange`/`shootCD` (Eagle) in inspector
