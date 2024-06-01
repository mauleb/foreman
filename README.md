This is very much a WIP. Buyer beware.

## Project Structure

**Foreman.CodeAnalysis**
- Lexing
- Syntax
- Binding

**Foreman.Console**
- the cli

**Foreman.Core**
Full scope TBD

**Foreman.Engine**
The actual template / job execution engine. The intent is to support Foreman as both a CLI and a Service. This does not process `.fm` files, the expectation is that you either write the foreman templates in the xml format outright, or you use the cli to build a collection of `.fm` files into their xml counterparts. The latter is strongly recommended in the grand scheme of things.

**Foreman.LanguageServer**
The LSP which powers the vscode extension
[specification](https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/)

**Foreman.LanguageServer.Protocol**
The implementation details of the open standard. This is all just mimics. We may be able to replace a bunch of this with open implementations if we look for it, I just haven't bothered yet. I wrote this mostly to play with the rpc serialization over stdio.

**vscode-foreman**
the vscode extension project. Baked within is an LSP client which is configured to run the debug dll LSP server defined in `Foreman.LanguageServer`.

## Tests
Run `dotnet test` from the root of the repo. They should all pass, however, some *might* be covering historic implementation details that deprecated through a long term discovery process. I believe they are all actually relevant still, but no promises.

## Running Locally

### the CLI
You can run the CLI by building the solution as a whole then cd-ing into that projects subdirectory and running the following command:
- `./bin/Debug/net8.0/Foreman.Console run`

That command is currently hardcoded to run the xml defined within the `src/Foreman.Console/example` directory

### the vscode extension
- Open up `src/vscode-foreman` in a separate vscode window
- Launch debugger client
    - this will open *another* window with the extension installed
    - the folder used by this extension window is configurable, I just don't recall how top of mind.
- Create `.fm` files which are to our current conventions, just a different file extension. `bootstrap.ps1` is no longer expected.

example file:
```
<job type="echo" wow="@{cool/beans}@"
  when="@{inputs/name}@" is="me"
>
  <message value="Hello, @{inputs/name}@!" />
  <message value="Hello, @{my.file.path/output}@!" />
  <hello>wow @{cool/beans}@</hello>
  <a>
    <b>cool</b>
  </a>
</job>
```

the current vscode extension does the following*:
- lex/parse the file as you type it
- display coloring based on semantics

*there are multiple issues that arise with this still, inconsistently