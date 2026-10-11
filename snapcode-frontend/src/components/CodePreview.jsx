import { useEffect, useRef, useState } from 'react';
import Prism from 'prismjs';
import 'prismjs/components/prism-dart';
import 'prismjs/themes/prism-tomorrow.css';
import './CodePreview.css';

export default function CodePreview({ code }) {
 const codeRef = useRef(null);
 const [copied, setCopied] = useState(false);

 useEffect(() => {
 if (codeRef.current) {
 Prism.highlightElement(codeRef.current);
 }
 }, [code]);

 async function handleCopy() {
 try {
 await navigator.clipboard.writeText(code);
 setCopied(true);
 setTimeout(() => setCopied(false), 2000);
 } catch {
 // Fallback
 const textarea = document.createElement('textarea');
 textarea.value = code;
 document.body.appendChild(textarea);
 textarea.select();
 document.execCommand('copy');
 document.body.removeChild(textarea);
 setCopied(true);
 setTimeout(() => setCopied(false), 2000);
 }
 }

 const lineCount = code ? code.split('\n').length : 0;

 return (
 <div className="code-preview">
 {/* Header */}
 <div className="code-preview-header">
 <div className="code-preview-title">
 <span className="code-file-icon"></span>
 <span>main.dart</span>
 </div>
 <div className="code-preview-actions">
 <span className="code-line-count">{lineCount} lines</span>
 <button
 className={`btn btn-ghost btn-sm ${copied ? 'copied' : ''}`}
 onClick={handleCopy}
 title="Copy to clipboard"
 >
 {copied ? ' Copied!' : ' Copy'}
 </button>
 </div>
 </div>

 {/* Code body */}
 <div className="code-preview-body">
 {/* Line numbers */}
 <div className="code-line-numbers">
 {code.split('\n').map((_, i) => (
 <span key={i}>{i + 1}</span>
 ))}
 </div>

 {/* Highlighted code */}
 <pre className="code-preview-pre">
 <code ref={codeRef} className="language-dart">
 {code || '// Drag blocks to generate Dart code...'}
 </code>
 </pre>
 </div>
 </div>
 );
}
