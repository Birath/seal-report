export async function fetchDestinyData(url) {
    const response = await fetch(url);
    if (response.ok) {
        return response.text();
    }
    return "";
}