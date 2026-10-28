"use strict";

async function loadRecords() {
  const status = document.getElementById("load-status");
  const body = document.getElementById("records");
  try {
    // This is a static local asset, not live document-processing status.
    const response = await fetch("sample-data.json", { cache: "no-store" });
    if (!response.ok) {
      throw new Error(`Sample data request failed: HTTP ${response.status}`);
    }
    const records = await response.json();
    // Fail visibly if the supplied fixture is missing or has the wrong shape.
    if (!Array.isArray(records) || records.length !== 3 ||
        records.some(record => !record || ["reference", "document", "status", "summary"]
          .some(field => typeof record[field] !== "string"))) {
      throw new Error("Sample data does not contain the three expected fictional records.");
    }
    const rows = records.map(record => {
      const row = document.createElement("tr");
      for (const field of ["reference", "document", "status"]) {
        const cell = document.createElement("td");
        // Treat sample values as text rather than interpreting them as HTML.
        cell.textContent = record[field];
        row.appendChild(cell);
      }
      const action = document.createElement("td");
      const button = document.createElement("button");
      button.textContent = "View details";
      button.setAttribute("aria-label", `View details for ${record.reference}`);
      // Details are already in the downloaded JSON; clicking does not call an API.
      button.addEventListener("click", () => {
        document.getElementById("details").textContent =
          `${record.reference}: ${record.summary}`;
      });
      action.appendChild(button);
      row.appendChild(action);
      return row;
    });
    body.replaceChildren(...rows);
    status.textContent = `${records.length} fictional records loaded.`;
  } catch (error) {
    status.textContent = "Unable to load sample data. Check the browser console and deployed files.";
    console.error("Dummy document tracker failed to load.", error);
  }
}

loadRecords();
