function getApiBaseUrl(): string {
  const isCodespaces = window.location.hostname.includes('.app.github.dev') || 
                       window.location.hostname.includes('.github.dev');
  
  if (isCodespaces) {
    const protocol = window.location.protocol;
    const codespacesHostname = window.location.hostname.replace(/-\d+\./, '-5110.');
    return `${protocol}//${codespacesHostname}/api/v1`;
  }
  
  return 'http://localhost:5110/api/v1';
}

export const environment = {
  apiBaseUrl: getApiBaseUrl(),
};

