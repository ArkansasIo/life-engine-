-- LifeEngine human biology / mutation research schema
-- MySQL 8+. Store only sequences that are lawful to retain and appropriately consented.
-- Synthetic examples should be marked source_type='synthetic'. Avoid direct identifiers.
CREATE DATABASE IF NOT EXISTS lifeengine_biology CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE lifeengine_biology;

CREATE TABLE IF NOT EXISTS biological_reference (
 reference_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 label VARCHAR(255) NOT NULL,
 organism_taxon VARCHAR(255) NOT NULL,
 source_type ENUM('synthetic','public_reference','research_sample','other') NOT NULL,
 accession VARCHAR(128) NULL,
 assembly_version VARCHAR(128) NULL,
 source_url TEXT NULL,
 license_notes TEXT NULL,
 retrieved_at DATETIME NULL,
 created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS sequence_record (
 sequence_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 reference_id BIGINT UNSIGNED NULL,
 sequence_kind ENUM('DNA','RNA') NOT NULL,
 sequence_label VARCHAR(255) NOT NULL,
 sequence_text LONGTEXT NOT NULL,
 is_synthetic BOOLEAN NOT NULL DEFAULT TRUE,
 sequence_length BIGINT UNSIGNED NOT NULL,
 checksum_sha256 CHAR(64) NULL,
 created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_sequence_reference FOREIGN KEY (reference_id) REFERENCES biological_reference(reference_id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS mutation_event (
 mutation_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 sequence_id BIGINT UNSIGNED NOT NULL,
 mutation_kind ENUM('substitution','insertion','deletion','other') NOT NULL,
 coordinate_system ENUM('zero_based_offset','one_based_position','reference_assembly') NOT NULL DEFAULT 'zero_based_offset',
 position_start BIGINT UNSIGNED NOT NULL,
 reference_allele TEXT NOT NULL,
 alternate_allele TEXT NOT NULL,
 model_name VARCHAR(255) NOT NULL,
 model_version VARCHAR(64) NOT NULL,
 random_seed BIGINT NULL,
 is_synthetic BOOLEAN NOT NULL DEFAULT TRUE,
 interpretation_status ENUM('not_assessed','research_hypothesis','evidence_reviewed') NOT NULL DEFAULT 'not_assessed',
 notes TEXT NULL,
 created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
 CONSTRAINT fk_mutation_sequence FOREIGN KEY (sequence_id) REFERENCES sequence_record(sequence_id) ON DELETE CASCADE,
 INDEX idx_mutation_position (sequence_id, position_start)
);

CREATE TABLE IF NOT EXISTS evidence_record (
 evidence_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 evidence_type ENUM('publication','database','experiment','hypothesis','documentation') NOT NULL,
 title VARCHAR(512) NOT NULL,
 authors_or_organization VARCHAR(512) NULL,
 doi VARCHAR(255) NULL,
 url TEXT NULL,
 publication_year SMALLINT NULL,
 evidence_summary TEXT NULL,
 limitations TEXT NULL,
 created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS mutation_evidence_link (
 mutation_id BIGINT UNSIGNED NOT NULL,
 evidence_id BIGINT UNSIGNED NOT NULL,
 relationship_note TEXT NULL,
 PRIMARY KEY (mutation_id, evidence_id),
 CONSTRAINT fk_link_mutation FOREIGN KEY (mutation_id) REFERENCES mutation_event(mutation_id) ON DELETE CASCADE,
 CONSTRAINT fk_link_evidence FOREIGN KEY (evidence_id) REFERENCES evidence_record(evidence_id) ON DELETE CASCADE
);
