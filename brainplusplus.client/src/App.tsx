import { useState } from 'react'
import './App.css'

interface CompileResult {
  success: boolean
  output?: string
  error?: string
  compilationOutput?: string
  exitCode?: number
}

function App() {
  const [code, setCode] = useState('#include <iostream>\n\nint main() {\n    std::cout << "Hello, Brain++!" << std::endl;\n    return 0;\n}')
  const [input, setInput] = useState('')
  const [output, setOutput] = useState('')
  const [error, setError] = useState('')
  const [isRunning, setIsRunning] = useState(false)

  const runCode = async () => {
    setIsRunning(true)
    setOutput('')
    setError('')

    try {
      const response = await fetch('/api/compiler/execute', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ code, input }),
      })

      const result: CompileResult = await response.json()

      if (result.success) {
        setOutput(result.output || '')
      } else {
        setError(result.error || result.compilationOutput || 'Unknown error')
      }
    } catch (err) {
      setError(`Failed to execute: ${err}`)
    } finally {
      setIsRunning(false)
    }
  }

  return (
    <div className="app-container">
      <h1>Brain++ C++ Compiler</h1>
      
      <div className="editor-section">
        <h2>Code</h2>
        <textarea
          className="code-editor"
          value={code}
          onChange={(e) => setCode(e.target.value)}
          placeholder="Write your C++ code here..."
          spellCheck={false}
        />
      </div>

      <div className="input-section">
        <h2>Input (stdin)</h2>
        <textarea
          className="input-box"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Enter input for your program..."
        />
      </div>

      <button 
        className="run-button" 
        onClick={runCode}
        disabled={isRunning}
      >
        {isRunning ? 'Running...' : 'Run Code'}
      </button>

      {output && (
        <div className="output-section">
          <h2>Output</h2>
          <pre className="output-box success">{output}</pre>
        </div>
      )}

      {error && (
        <div className="output-section">
          <h2>Error</h2>
          <pre className="output-box error">{error}</pre>
        </div>
      )}
    </div>
  )
}

export default App
