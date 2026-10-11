# Employee Management Web Server

## Overview
This project is a custom HTTP Web Server written from scratch in C, accompanied by a frontend HTML page. It demonstrates the fundamental, low-level mechanics of how the internet works by bypassing modern web frameworks (like Node.js, Python/Django, or PHP) and directly manipulating TCP sockets to serve web pages and act as a RESTful API.

## Deep Concepts Explored

### 1. TCP/IP and Sockets
At the lowest level, web browsers and servers communicate over **TCP (Transmission Control Protocol)**. A socket is an endpoint for sending or receiving data across a computer network. 
* **`socket()`**: Requests a network socket from the Operating System. We use `AF_INET` for IPv4 and `SOCK_STREAM` to specify a reliable, two-way TCP connection.
* **`bind()`**: Reserves a specific port number (calculated as `6000 + (ID % 800)`) on the machine. This tells the OS to route all incoming network traffic for this port directly to our C program.
* **`listen()` & `accept()`**: `listen()` opens the gates for traffic, and `accept()` puts the C program to sleep (blocks) until a client (browser) connects. When a connection happens, it creates a dedicated communication channel (`clientSocket`) for that specific user.

### 2. The HTTP Protocol (Plain Text)
People often think HTTP is a complex binary format, but it is fundamentally just a formatted plain-text conversation over a TCP socket.
When you fetch a webpage, the browser sends raw text that looks like this:
```http
GET / HTTP/1.1
Host: 127.0.0.1:6434
Accept: text/html
```
Our C server reads this text buffer using `recv()` and parses it using standard string manipulation (`sscanf`). It pulls out the "Method" (`GET`) and the "Path" (`/`).

To send data back, the server must format its text exactly as the strict HTTP protocol demands, requiring exact `\r\n` (Carriage Return + Line Feed) line endings:
```http
HTTP/1.1 200 OK\r\n
Content-Type: text/html\r\n
Content-Length: 1024\r\n
\r\n
<html>...</html>
```
The final blank line (`\r\n\r\n`) is a critical deep concept in HTTP: it is the exact signal the browser uses to know that the headers have finished and the actual content (the body) is beginning.

### 3. In-Memory Database and Routing
* **Routing**: The server acts as a traffic cop. It inspects the HTTP request to determine the user's intent. `GET /` serves the UI, `POST` creates data, `GET /?empid=X` retrieves data, and `DELETE /?empid=X` removes data.
* **Data Storage**: A static array of `struct employeeData` holds the records in RAM. Because it is in memory, all data is lost when the C program closes (standard for simple state management exercises).
* **Deletion Logic**: To delete an item from the middle of the array, the server performs a memory shift. It loops from the deleted index to the end, copying `database[i+1]` into `database[i]`, effectively collapsing the gap and keeping the data contiguous.

### 4. Client-Side Rendering and Async Fetch
Instead of older web patterns where a `<form>` submission forces the entire web page to turn white and reload, this project uses modern SPA (Single Page Application) concepts.
When a user clicks "create", the browser does not reload. Instead, JavaScript's `fetch()` API opens a secret, asynchronous HTTP connection in the background. It sends the `POST` text to the C server, waits for the plain text response, and displays it via an `alert()` pop-up, leaving the user interface completely undisturbed.

## File Structure
* **`server.c`**: The backend engine. Handles network I/O, routing, HTTP parsing, and database management.
* **`index.html`**: The graphical user interface. Uses modern JavaScript to send asynchronous background requests to the C server.

## Running the Project
1. Open your WSL (Windows Subsystem for Linux) terminal.
2. Navigate to the directory containing these files.
3. Compile the C code: 
   `gcc server.c -o server`
4. Run the compiled server: 
   `./server`
5. Open a web browser on your Windows machine and navigate to the address printed in the terminal (e.g., `http://127.0.0.1:6434`).
