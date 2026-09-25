// Build production qua node thay vì lệnh `ng`:
// - Máy build của hosting chặn chạy ng.cmd ("blocked by group policy") → gọi thẳng ng.js bằng node.
// - Angular có thể treo khi đọc stdin trên máy build → chạy tiến trình con, không cho đọc stdin.
// - In nhịp mỗi 5 giây để log build không im lặng quá lâu.
import { spawn } from 'node:child_process';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const ngJs = require.resolve('@angular/cli/bin/ng.js');
const args = [ngJs, 'build', ...process.argv.slice(2)];

const started = Date.now();
const child = spawn(process.execPath, args, {
  stdio: ['ignore', 'inherit', 'inherit'],
  env: { ...process.env, NG_CLI_ANALYTICS: 'false', CI: 'true' },
});

const heartbeat = setInterval(() => {
  console.log(`[build] đang chạy... ${Math.round((Date.now() - started) / 1000)}s`);
}, 5000);

child.on('exit', (code, signal) => {
  clearInterval(heartbeat);
  const exitCode = code ?? 1;
  console.log(`[build] kết thúc sau ${Math.round((Date.now() - started) / 1000)}s, mã thoát ${exitCode}${signal ? `, signal ${signal}` : ''}`);
  process.exit(exitCode);
});

child.on('error', (err) => {
  clearInterval(heartbeat);
  console.error('[build] không chạy được Angular CLI:', err);
  process.exit(1);
});
