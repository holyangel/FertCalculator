window.fertcalc = {
  saveLocal: function (key, value) {
    try { localStorage.setItem(key, value); return true; } catch { return false; }
  },
  loadLocal: function (key) {
    return localStorage.getItem(key);
  },
  removeLocal: function (key) {
    localStorage.removeItem(key);
  },
  downloadXml: function (filename, xml) {
    const blob = new Blob([xml], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  },
  readFile: function (inputId) {
    const input = document.getElementById(inputId);
    if (!input || !input.files || input.files.length === 0) return Promise.resolve(null);
    return input.files[0].text();
  }
};
