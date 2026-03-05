# Agentic Coding Guidelines for TurnBasedGame

This document provides context, conventions, and instructions for coding agents operating within this Unity project.

## 1. Project Overview & Architecture

- **Engine:** Unity (2022.x / 6000.x stream implied).
- **Language:** C# (.NET Standard 2.1 compliant).
- **Core Frameworks:**
  - **Networking:** Unity Netcode for GameObjects (NGO).
  - **Dependency Injection:** Reflex.
  - **Serialization:** MessagePack-CSharp.
  - **Services:** Unity Gaming Services (Authentication, Lobby, Relay).
  - **UI/Tweening:** DOTween.

### Dependency Injection (Reflex)
- Use `IInstaller` implementations (e.g., `ProjectInstaller.cs`) to register dependencies.
- Prefer constructor injection or `[Inject]` property injection where appropriate, though `MonoBehaviour` injection often relies on Reflex's scene hooks.
- Register services as Singletons/Transients in the `InstallBindings` method.

## 2. Environment & Commands

Since this is a Unity project, standard CLI build/test commands (like `npm test`) are **not available** by default.

### Running Tests
- **Method:** Tests are executed via the **Unity Test Runner** window in the Editor.
- **Single Test:** Open `Window > General > Test Runner`, locate the specific test, and click "Run Selected".
- **CLI Alternative:** Currently, there is no configured shell script to run a single test from the command line.

### Code Compilation
- Unity compiles code automatically on file save/focus.
- **Agent Note:** Ensure all generated code is free of syntax errors. Use `dotnet build` on the solution file (`TurnBasedGame.sln`) if available to verify syntax, but rely on user feedback for Unity-specific compilation errors.

## 3. Code Style & Conventions

Adhere strictly to the following C# and Unity conventions.

### Formatting
- **Braces:** Use **Allman** style (opening brace on a new line).
  ```csharp
  // Correct
  if (condition)
  {
      DoSomething();
  }
  ```
- **Indentation:** Use 4 spaces (standard Visual Studio/Unity default).
- **Namespaces:** Use global namespace for scripts unless a specific module structure is strictly enforced (current codebase uses global namespace).

### Naming Conventions
- **Classes/Structs/Enums:** `PascalCase` (e.g., `ApplicationController`).
- **Methods:** `PascalCase` (e.g., `StartGame`, `OnValueChanged`).
- **Properties:** `PascalCase` (e.g., `IsConnected`).
- **Interfaces:** `IPascalCase` (e.g., `IViewModel`).
- **Serialized Fields:** `camelCase` with `[SerializeField]` attribute.
  ```csharp
  [SerializeField] private string sceneNameToLoad;
  ```
- **Private/Internal Fields:** `_camelCase` (underscore prefix) is preferred for non-serialized backing fields.
  ```csharp
  private int _retryCount;
  ```
  *(Note: Legacy code may contain `m_` prefix; prefer `_` for new code).*

### Unity Specifics
- **Serialization:** ALWAYS use `[SerializeField] private` instead of `public` fields for Inspector exposure to maintain encapsulation.
- **Coroutines:** Use `IEnumerator` for temporal logic where `async/await` is not feasible, though `UniTask` (if available) or standard `Task` is preferred for non-Unity-lifecycle async operations.
- **Null Checking:** Be careful with Unity Object null checks (`== null`) vs standard C# null checks.

## 4. Testing Strategy

- **Location:** Tests should reside in `Assets/Tests/Editor` (EditMode) or `Assets/Tests/PlayMode` (PlayMode).
- **Framework:** Unity Test Framework (NUnit based).
- **Structure:**
  ```csharp
  using NUnit.Framework;
  using UnityEngine.TestTools;
  using System.Collections;

  public class ExampleTests
  {
      [Test]
      public void SimpleLogicTest()
      {
          Assert.AreEqual(2, 1 + 1);
      }

      [UnityTest]
      public IEnumerator GameLogicCoroutineTest()
      {
          yield return null; // Wait for frame
          Assert.IsTrue(true);
      }
  }
  ```

## 5. File System & Assets

- **Meta Files:** Unity generates `.meta` files for every asset.
  - **Agent Action:** Do NOT manually create or edit `.meta` files unless you are performing a file move/copy operation where you must preserve the GUID. Generally, let the Unity Editor handle meta file generation.
- **Path Handling:** Always use forward slashes `/` for paths in code, even on Windows, to ensure cross-platform compatibility within Unity.

## 6. Language

- **Comments:** Write all new comments in **English**.
  - *Note:* Existing codebase may contain French comments (e.g., in `Bindable.cs`). Do not translate existing comments unless modifying the specific code block.

## 7. Error Handling

- Use `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError` for runtime feedback.
- Fail gracefully in `Awake` or `Start` if critical dependencies (via Reflex or Inspector) are missing.
- Example:
  ```csharp
  private void Awake()
  {
      if (_dependency == null)
      {
          Debug.LogError($"{nameof(MyClass)}: Dependency is missing!");
          enabled = false;
          return;
      }
  }
  ```
